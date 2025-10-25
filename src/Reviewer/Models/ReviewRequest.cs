namespace Reviewer.Models
{
    public class ReviewRequest
    {
        public required string BaseRef { get; set; }
        public required string HeadRef { get; set; }
        public required string DiffContent { get; set; }
    }
}
