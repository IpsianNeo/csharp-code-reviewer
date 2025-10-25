namespace Reviewer.Helpers
{
    public static class PathHelper
    {
        public static string FindRepoRootWithTools()
        {
            string? currentDir = AppContext.BaseDirectory;
            while (currentDir != null)
            {
                string candidate = Path.Combine(currentDir, "tools", "mcp_manifest.yml");
                if (File.Exists(candidate))
                {
                    return currentDir; // repo root found
                }
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            throw new FileNotFoundException("Could not find 'tools/mcp_manifest.yml' in any parent folder.");
        }

        public static string GetManifestPath()
        {
            var repoRoot = FindRepoRootWithTools();
            return Path.Combine(repoRoot, "tools", "mcp_manifest.yml");
        }
    }
}
