using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIDebuggerCli.Models;

namespace AIDebuggerCli.Services
{
    public class FileScannerService
    {
        private readonly AppConfig _config;

        public FileScannerService(AppConfig config)
        {
            _config = config;
        }

        public List<string> ScanDirectory(string targetPath, out List<string> skippedLargeFiles)
        {
            var matchedFiles = new List<string>();
            skippedLargeFiles = new List<string>();

            if (!Directory.Exists(targetPath))
                return matchedFiles;

            ScanRecursive(targetPath, matchedFiles, skippedLargeFiles);
            return matchedFiles;
        }

        private void ScanRecursive(string currentDir, List<string> matched, List<string> skipped)
        {
            string dirName = Path.GetFileName(currentDir);
            if (_config.IgnoredFolders.Any(f => string.Equals(f, dirName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            try
            {
                foreach (string file in Directory.GetFiles(currentDir))
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (_config.SupportedExtensions.Contains(extension))
                    {
                        var fileInfo = new FileInfo(file);
                        long maxBytes = _config.MaxFileSizeKb * 1024;

                        if (fileInfo.Length > maxBytes)
                        {
                            skipped.Add(file);
                        }
                        else
                        {
                            matched.Add(file);
                        }
                    }
                }

                foreach (string subDir in Directory.GetDirectories(currentDir))
                {
                    ScanRecursive(subDir, matched, skipped);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}