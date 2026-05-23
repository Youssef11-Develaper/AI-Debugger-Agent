using System.Text.Json;
using AIDebuggerCli.Core;
using AIDebuggerCli.Models;
using DotNetEnv;
using Spectre.Console;

namespace AIDebuggerCli
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("AI ENGINE CLI").Color(Color.DeepSkyBlue1));
            AnsiConsole.MarkupLine("[bold darkcyan]System Engine Initializing Framework Architecture...[/]\n");

            Env.Load();
            string apiKey = Environment.GetEnvironmentVariable("AGENTROUTER_API_KEY") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                AnsiConsole.MarkupLine("[bold red]CRITICAL EXCEPTION: Environment context key 'AGENTROUTER_API_KEY' is missing in .env structure.[/]");
                return;
            }

            AppConfig runtimeConfig;
            try
            {
                string configContent = await File.ReadAllTextAsync("config.json");
                runtimeConfig = JsonSerializer.Deserialize<AppConfig>(configContent) ?? new AppConfig();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning: Could not parse config.json ({ex.Message}). Using default parameters.[/]");
                runtimeConfig = new AppConfig();
            }

            string targetScanDirectory = AnsiConsole.Ask<string>("[bold white]Enter targeted engineering filesystem directory absolute path:[/]");

            if (!Directory.Exists(targetScanDirectory))
            {
                AnsiConsole.MarkupLine("[bold red]ERROR: Path boundary validation failed. Target location missing.[/]");
                return;
            }

            var activeExecutionMode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Select operational orchestration analysis profile criteria:[/]")
                    .PageSize(5)
                    .AddChoices(new[] { "1. Debug Mode", "2. Security Scan Mode", "3. Optimization Mode" }));

            AnalysisMode parsedMode = activeExecutionMode.Substring(0, 1) switch
            {
                "1" => AnalysisMode.Debug,
                "2" => AnalysisMode.Security,
                "3" => AnalysisMode.Optimization,
                _ => AnalysisMode.Debug
            };

            try
            {
                var dynamicEngineCluster = new Engine(runtimeConfig, apiKey);
                await dynamicEngineCluster.RunAsync(targetScanDirectory, parsedMode);
            }
            catch (Exception globalEx)
            {
                AnsiConsole.WriteException(globalEx);
            }
        }
    }
}