namespace Reviewer.Models
{
    // Minimal manifest classes
    public class McpManifest
    {
        public string? Version { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, McpTool> Tools { get; set; } = new();
    }

    public class McpTool
    {
        public string? Description { get; set; }
        public string? Shell { get; set; }         // e.g. "powershell", "bash", "pwsh"
        public string? Command { get; set; }       // optional, not used if Shell+Args used
        public string? Args { get; set; }          // argument template (with {placeholders})
        public string? Output { get; set; }        // "text" or "none"
        public int Timeout_Seconds { get; set; }  // optional
    }
}
