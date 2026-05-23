using System.Collections.Generic;

namespace AIDebuggerCli.Models
{
    public class AppConfig
    {
        public string Model { get; set; } = "deepseek/deepseek-chat";
        public int MaxFileSizeKb { get; set; } = 500;
        public List<string> IgnoredFolders { get; set; } = new();
        public List<string> SupportedExtensions { get; set; } = new();
        public bool BackupEnabled { get; set; } = true;
        public int RetryCount { get; set; } = 3;
        public bool ParallelProcessing { get; set; } = true;
    }
}