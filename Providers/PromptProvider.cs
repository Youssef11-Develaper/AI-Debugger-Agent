using AIDebuggerCli.Models;

namespace AIDebuggerCli.Providers
{
    public static class PromptProvider
    {
        public static string GetSystemInstruction(AnalysisMode mode)
        {
            string objectivePrompt = mode switch
            {
                AnalysisMode.Debug => 
                    "Identify and fix compilation/syntax errors, syntax bugs, logical issues, type mismatches, and runtime exceptions.",
                
                AnalysisMode.Security => 
                    "Detect and fix critical vulnerabilities including SQL injection, hardcoded API keys/secrets, unsafe eval executions, insecure code patterns, and XSS risks.",
                
                AnalysisMode.Optimization => 
                    "Refactor the code to improve performance runtime latency, readability execution flow, memory usage optimization, and clean code structure adherence.",
                
                _ => "Analyze and fix bugs within the provided code segment."
            };

            return "You are an elite senior architect and principal AI automated software engineer.\n" +
                   $"Your exact objective is to perform a code transformation focusing on: {objectivePrompt}\n\n" +
                   "CRITICAL INSTRUCTIONS:\n" +
                   "1. Preserve 100% of the core functional domain business rules and logic unless fixing a verified structural flaw.\n" +
                   "2. Return nothing but cleanly refactored, production-ready execution strings.\n" +
                   "3. You MUST respond ONLY with a single, strictly valid JSON object. No conversational headers, no footers, and absolutely no markdown enclosing fences (e.g., do NOT wrap your JSON in ```json blocks).\n\n" +
                   "TARGET JSON SCHEMA COMPLIANCE SPECIFICATION:\n" +
                   "{\n" +
                   "  \"explanation\": \"A technical, detailed itemized bullet list outlining the structural defects identified and the targeted engineering mutations applied, entirely written in English.\",\n" +
                   "  \"fixedCode\": \"The full text of the transformed execution source code string.\"\n" +
                   "}";
        }
    }
}