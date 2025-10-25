namespace Reviewer
{
    public class Orchestrator
    {
        private readonly McpManager _mcp;

        public Orchestrator(string manifestPath)
        {
            _mcp = new McpManager(manifestPath);
        }

        public void RunReview(string baseRef, string headRef)
        {
            Console.WriteLine("🚀 Starting AI code review process...");
            Console.WriteLine("Step 1: Fetching Git diff via MCP tool...");

            try
            {
                // Get the diff between branches
                var diff = _mcp.RunTool("git_diff", new()
                {
                    { "baseRef", baseRef },
                    { "headRef", headRef }
                });

                if (string.IsNullOrWhiteSpace(diff))
                {
                    Console.WriteLine("No differences found — nothing to review.");
                    return;
                }

                Console.WriteLine("Step 2: Sending diff to AI reviewer...");

                // Pass diff to AI Reviewer logic
                var aiReviewer = new ReviewerEngine();
                var reviewResult = aiReviewer.AnalyzeDiff(diff);

                Console.WriteLine("Step 3: Saving review results...");

                var outputPath = Path.Combine(AppContext.BaseDirectory, "REVIEW_SUMMARY.md");
                // Save results via MCP tool
                _mcp.RunTool("git_comment", new()
                {
                    { "outputPath", outputPath },
                    { "content", reviewResult }
                });

                if (string.IsNullOrWhiteSpace(reviewResult))
                {
                    Console.WriteLine("⚠️ AI reviewer returned empty result.");
                    return;
                }
                Console.WriteLine("✅ Review complete! Output saved to REVIEW_SUMMARY.md");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Console.Error.WriteLine("❌ Failed to fetch Git diff: " + ex.Message);
                return;
            }
        }
    }
}
