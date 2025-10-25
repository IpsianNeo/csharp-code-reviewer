using System.Diagnostics;
using System.Text.Json;

namespace Reviewer
{
    public class Reviewer
    {
        private readonly LlmClient _llm;
        private readonly string _promptTemplate;

        public Reviewer()
        {
            _llm = new LlmClient("http://localhost:11434"); // Ollama default or LocalAI port
            _promptTemplate = File.ReadAllText("src/Reviewer/prompts/review_prompt.txt");
        }

        public async Task<string> RunReviewAsync(string baseRef, string headRef)
        {
            // 1. Get diff using tool
            var diff = RunTool("./tools/git_diff.ps1", $"{baseRef} {headRef}");
            if (string.IsNullOrWhiteSpace(diff))
            {
                return "No diff found.";
            }

            // 2. Build prompt
            var prompt = _promptTemplate.Replace("{diff}", diff);

            // 3. Call LLM
            var jsonResponse = await _llm.GetCompletionJsonAsync(prompt);

            // 4. Validate JSON (basic)
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
            }
            catch (Exception ex)
            {
                // If model failed to produce valid JSON, log and fail-safe
                File.WriteAllText("review_error.log", jsonResponse);
                return $"LLM returned invalid JSON: {ex.Message}";
            }

            // 5. Write review artifact locally
            File.WriteAllText("REVIEW_SUMMARY.json", jsonResponse);

            // 6. Convert to Markdown summary and call git_comment tool
            var md = ConvertJsonToMarkdown(jsonResponse);
            RunTool("./tools/git_comment.ps1", $"REVIEW_SUMMARY.md \"{md}\"");

            return "Review completed. REVIEW_SUMMARY.md generated.";
        }

        private static string RunTool(string script, string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = script,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi);
            var outp = p?.StandardOutput.ReadToEnd();
            var err = p?.StandardError.ReadToEnd();
            p?.WaitForExit();
            if (!string.IsNullOrEmpty(err)) Console.Error.WriteLine(err);
            return outp?.Trim() ?? string.Empty;
        }

        private static string ConvertJsonToMarkdown(string json)
        {
            // Basic conversion; you can expand
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var summary = root.GetProperty("summary").GetString();
            var md = $"# Review Summary\n\n**{summary}**\n\n";
            if (root.TryGetProperty("issues", out var issues))
            {
                foreach (var item in issues.EnumerateArray())
                {
                    var file = item.GetProperty("file").GetString();
                    var line = item.GetProperty("line").GetInt32();
                    var msg = item.GetProperty("message").GetString();
                    var rec = item.GetProperty("recommendation").GetString();
                    md += $"## {file}:{line}\n- {msg}\n- Recommendation: {rec}\n\n";
                }
            }
            return md;
        }

        private static string GetGitDiff(string baseRef, string headRef)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-File tools/git_diff.ps1 {baseRef} {headRef}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            string output = process?.StandardOutput.ReadToEnd().Trim() ?? string.Empty;
            process?.WaitForExit();
            return output;
        }

    }
}
