namespace Reviewer.Models
{
    public class McpManifest
    {
        public required string Version { get; set; }
        public required string Description { get; set; }
        public required Dictionary<string, McpTool> Tools { get; set; }
    }

    public class McpTool
    {
        public required string Description { get; set; }
        public required string Command { get; set; }
        public required string Args { get; set; }
        public required string Output { get; set; }
    }
}
