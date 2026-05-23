using System;
using System.IO;

namespace AIDebuggerCli.Services
{
    public class BackupService
    {
        public string CreateBackup(string filePath)
        {
            try
            {
                string backupPath = filePath + ".bak";
                File.Copy(filePath, backupPath, true);
                return backupPath;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to initialize safety rollback state: {ex.Message}", ex);
            }
        }

        public void RestoreBackup(string filePath)
        {
            string backupPath = filePath + ".bak";
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, filePath, true);
                File.Delete(backupPath);
            }
        }

        public void ClearBackup(string filePath)
        {
            string backupPath = filePath + ".bak";
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
    }
}