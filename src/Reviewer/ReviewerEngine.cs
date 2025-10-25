using System.Text;

namespace Reviewer
{
    public class ReviewerEngine
    {
        public string AnalyzeDiff(string diff)
        {
            // Placeholder: later replaced with AI model logic
            var sb = new StringBuilder();
            sb.AppendLine("# Automated C# Code Review");
            sb.AppendLine("### Summary of Changes Analyzed:");
            sb.AppendLine("```diff");
            sb.AppendLine(diff);
            sb.AppendLine("```");
            sb.AppendLine("### Preliminary Comments:");
            sb.AppendLine("- Check naming conventions.");
            sb.AppendLine("- Ensure async methods use proper await.");
            sb.AppendLine("- Validate null-handling in data models.");

            return sb.ToString();
        }
    }
}
