using System;
using System.Text.RegularExpressions;

namespace AIDebuggerCli.Utils
{
    public static class JsonCleanupUtil
    {
        public static string ExtractValidJson(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return string.Empty;

            string cleaned = rawInput.Trim();

            if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(7).Trim();
            }
            else if (cleaned.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(3).Trim();
            }

            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();
            }

            int firstBrace = cleaned.IndexOf('{');
            int lastBrace = cleaned.LastIndexOf('}');

            if (firstBrace != -1 && lastBrace != -1 && lastBrace > firstBrace)
            {
                cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            return cleaned;
        }
    }
}