namespace Reviewer.Models
{
    public class ReviewResponse
    {
        public required string File { get; set; }
        public List<Issue> Issues { get; set; } = new List<Issue>();
    }

    public class Issue
    {
        public required string RuleId { get; set; }
        public int Line { get; set; }
        public required string Description { get; set; }
    }
}
