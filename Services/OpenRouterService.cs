using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AIDebuggerCli.Models;
using AIDebuggerCli.Utils;

namespace AIDebuggerCli.Services
{
    public class OpenRouterService
    {
        private readonly AppConfig _config;
        private readonly string _apiKey;
        private const string BaseUrl = "https://openrouter.ai/api/v1/chat/completions";
        public OpenRouterService(AppConfig config, string apiKey)
        {
            _config = config;
            _apiKey = apiKey;
        }

        public async Task<AiResponse> ExecutePayloadTransactionAsync(string codeSegment, string fileExtension, string instructionPrompt)
{
    int executionAttempts = 0;
    TimeSpan structuralBackoffDelay = TimeSpan.FromSeconds(2);

    while (executionAttempts < _config.RetryCount)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
            client.DefaultRequestHeaders.Add("X-Title", "AI Code Debugger Core CLI");

            var dynamicPayload = new
            {
                model = _config.Model,
                messages = new[]
                {
                    new { role = "system", content = instructionPrompt },
                    new { role = "user", content = $"File Extension Context: {fileExtension}\n\nRaw Source Code Content:\n{codeSegment}" }
                },
                temperature = 0.1
            };

            string jsonText = JsonSerializer.Serialize(dynamicPayload);
            var httpContextContent = new StringContent(jsonText, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(BaseUrl, httpContextContent);
            string rawResponseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using JsonDocument wrapperDoc = JsonDocument.Parse(rawResponseString);
                string rawInternalOutput = wrapperDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                Spectre.Console.AnsiConsole.MarkupLine($"[yellow]=== RAW AI OUTPUT FOR {fileExtension.ToUpper()} ===[/]");
                Console.WriteLine(rawInternalOutput);
                Spectre.Console.AnsiConsole.MarkupLine("[yellow]===================================[/]");

                string sanitizedJson = JsonCleanupUtil.ExtractValidJson(rawInternalOutput);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var processedResult = JsonSerializer.Deserialize<AiResponse>(sanitizedJson, options);
                
                if (processedResult != null)
                {
                    return processedResult;
                }
            }
            else
            {
                Spectre.Console.AnsiConsole.MarkupLine($"[red]API Server Error: {response.StatusCode} - {rawResponseString}[/]");
            }
        }
        catch (Exception ex)
        {
            Spectre.Console.AnsiConsole.MarkupLine($"[bold red]❌ Parsing/Runtime Error: {ex.Message}[/]");
            if (ex.InnerException != null)
            {
                Spectre.Console.AnsiConsole.MarkupLine($"[red]Inner Error: {ex.InnerException.Message}[/]");
            }
        }

        executionAttempts++;
        if (executionAttempts < _config.RetryCount)
        {
            Spectre.Console.AnsiConsole.MarkupLine($"[yellow]⚠️ Attempt {executionAttempts} failed. Retrying in {structuralBackoffDelay.TotalSeconds}s...[/]");
            await Task.Delay(structuralBackoffDelay);
            structuralBackoffDelay = structuralBackoffDelay.Multiply(2);
        }
    }

    throw new InvalidOperationException($"API failed after reaching maximum threshold limits of {_config.RetryCount} retry attempts.");
}
    }
}