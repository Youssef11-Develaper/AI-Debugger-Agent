using System;
using System.IO;
using System.Text;

namespace AIDebuggerCli.Services
{
    public class LoggingService
    {
        private readonly StringBuilder _logBuilder = new();

        public void InitializeLog(string mode)
        {
            _logBuilder.AppendLine("=====================================================================================");
            _logBuilder.AppendLine($"   CORE ENTERPRISE ENGINE SYSTEM AUDIT LOG RUNTIME - TIMEOUT DOMAIN: {DateTime.Now}");
            _logBuilder.AppendLine($"   EXECUTION RUN ANALYSIS CONFIGURATION MODE: {mode}");
            _logBuilder.AppendLine("=====================================================================================\n");
        }

        public void LogSuccess(string filename, string actions)
        {
            lock (_logBuilder)
            {
                _logBuilder.AppendLine($"[SUCCESS] Target Object: {filename}");
                _logBuilder.AppendLine("Mutations Implemented:");
                _logBuilder.AppendLine(actions);
                _logBuilder.AppendLine(new string('-', 85) + "\n");
            }
        }

        public void LogFailure(string filename, string rootError)
        {
            lock (_logBuilder)
            {
                _logBuilder.AppendLine($"[CRITICAL FAILURE] Target Object: {filename}");
                _logBuilder.AppendLine($"Exception Details: {rootError}");
                _logBuilder.AppendLine(new string('-', 85) + "\n");
            }
        }

        public void LogSkipped(string filename, string reasoning)
        {
            lock (_logBuilder)
            {
                _logBuilder.AppendLine($"[SKIPPED PATH] Target Object: {filename} Reason: {reasoning}");
                _logBuilder.AppendLine(new string('-', 85) + "\n");
            }
        }

        public async Task WriteLogToFileAsync(string directoryPath)
        {
            string outputDestination = Path.Combine(directoryPath, "debug_log.txt");
            await File.WriteAllTextAsync(outputDestination, _logBuilder.ToString());
        }
    }
}