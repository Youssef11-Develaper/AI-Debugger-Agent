using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AIDebuggerCli.Models;
using AIDebuggerCli.Providers;
using AIDebuggerCli.Services;
using Spectre.Console;

namespace AIDebuggerCli.Core
{
    public class Engine
    {
        private readonly AppConfig _config;
        private readonly OpenRouterService _apiService;
        private readonly FileScannerService _scannerService;
        private readonly BackupService _backupService;
        private readonly LoggingService _loggingService;

        public Engine(AppConfig config, string apiKey)
        {
            _config = config;
            _apiService = new OpenRouterService(config, apiKey);
            _scannerService = new FileScannerService(config);
            _backupService = new BackupService();
            _loggingService = new LoggingService();
        }

        public async Task RunAsync(string directoryPath, AnalysisMode runMode)
        {
            _loggingService.InitializeLog(runMode.ToString().ToUpper());

            AnsiConsole.MarkupLine($"[bold blue]Scanning target file architecture tree...[/]");
            var targetFiles = _scannerService.ScanDirectory(directoryPath, out var largeFiles);

            foreach (var skippedPath in largeFiles)
            {
                _loggingService.LogSkipped(Path.GetFileName(skippedPath), "Exceeded file size capacity limitations.");
            }

            int totalCount = targetFiles.Count + largeFiles.Count;
            int totalProcessedCount = targetFiles.Count;
            int successfulMutations = 0;
            int failedMutations = 0;

            if (totalProcessedCount == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No supported structural source assets detected within directory boundaries.[/]");
                await _loggingService.WriteLogToFileAsync(directoryPath);
                return;
            }

            string targetInstructionPrompt = PromptProvider.GetSystemInstruction(runMode);
            var runtimeStopwatch = Stopwatch.StartNew();

            AnsiConsole.MarkupLine($"[bold green]Processing pipeline active. Targets detected: {totalProcessedCount} files.[/]\n");

            await AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn()
                })
                .StartAsync(async context =>
                {
                    var operationalTask = context.AddTask("[cyan]Processing Execution Files[/]", maxValue: totalProcessedCount);

                    if (_config.ParallelProcessing)
                    {
                        var taskCollection = new List<Task>();
                        foreach (var targetFile in targetFiles)
                        {
                            var concurrentJob = Task.Run(async () =>
                            {
                                bool state = await ProcessSingleFileAsync(targetFile, targetInstructionPrompt);
                                lock (operationalTask)
                                {
                                    if (state) successfulMutations++; else failedMutations++;
                                    operationalTask.Increment(1);
                                }
                            });
                            taskCollection.Add(concurrentJob);
                        }
                        await Task.WhenAll(taskCollection);
                    }
                    else
                    {
                        foreach (var targetFile in targetFiles)
                        {
                            bool state = await ProcessSingleFileAsync(targetFile, targetInstructionPrompt);
                            if (state) successfulMutations++; else failedMutations++;
                            operationalTask.Increment(1);
                        }
                    }
                });

            runtimeStopwatch.Stop();
            await _loggingService.WriteLogToFileAsync(directoryPath);

            RenderSummaryReportDashboard(totalCount, successfulMutations, failedMutations, largeFiles.Count, runtimeStopwatch.Elapsed);
        }

        private async Task<bool> ProcessSingleFileAsync(string path, string prompt)
        {
            string currentFileName = Path.GetFileName(path);
            string backupReferencePath = string.Empty;

            try
            {
                if (_config.BackupEnabled)
                {
                    backupReferencePath = _backupService.CreateBackup(path);
                }

                string rawSourceContent = await File.ReadAllTextAsync(path);
                string fileExtension = Path.GetExtension(path).Replace(".", "");

                var executionResponse = await _apiService.ExecutePayloadTransactionAsync(rawSourceContent, fileExtension, prompt);

                await File.WriteAllTextAsync(path, executionResponse.FixedCode);
                _loggingService.LogSuccess(currentFileName, executionResponse.Explanation);

                if (_config.BackupEnabled)
                {
                    _backupService.ClearBackup(path);
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogFailure(currentFileName, ex.Message);

                if (_config.BackupEnabled && !string.IsNullOrEmpty(backupReferencePath))
                {
                    _backupService.RestoreBackup(path);
                }

                return false;
            }
        }

        private void RenderSummaryReportDashboard(int total, int fixedCount, int failed, int skipped, TimeSpan runtime)
        {
            AnsiConsole.WriteLine();
            var interfaceTable = new Table()
                .Title("[bold white]CORE CLUSTER EXECUTION METRIC DASHBOARD[/]")
                .Border(TableBorder.Square)
                .AddColumn(new TableColumn("[bold yellow]Metric Description[/]").Centered())
                .AddColumn(new TableColumn("[bold yellow]Value Matrix[/]").Centered());

            interfaceTable.AddRow("Total File Scanned Assets", total.ToString());
            interfaceTable.AddRow("[green]Successfully Mutated Assets[/]", fixedCount.ToString());
            interfaceTable.AddRow("[red]Failed Code Mutations[/]", failed.ToString());
            interfaceTable.AddRow("[blue]Skipped File Targets[/]", skipped.ToString());
            interfaceTable.AddRow("Total Processing Engine Pipeline Duration", $"{runtime.TotalSeconds:F2} seconds");

            AnsiConsole.Write(interfaceTable);
        }
    }
}