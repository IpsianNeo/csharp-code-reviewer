using Reviewer.Helpers;
using Reviewer.Models;

using System.Diagnostics;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Reviewer
{
    public class McpManager
    {
        private readonly McpManifest _manifest;

        public McpManager(string manifestPath)
        {
            var yaml = File.ReadAllText(manifestPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _manifest = deserializer.Deserialize<McpManifest>(yaml);
        }

        public string RunTool(string toolName, Dictionary<string, string> parameters)
        {
            if (!_manifest.Tools.ContainsKey(toolName))
                throw new Exception($"Tool {toolName} not found in manifest.");

            var tool = _manifest.Tools[toolName];
            var args = tool.Args;
            foreach (var kv in parameters)
                args = args.Replace($"{{{kv.Key}}}", kv.Value);

            // Get absolute path to script
            var repoRoot = PathHelper.FindRepoRootWithTools(); // use same helper as before
            string scriptPath = Path.Combine(repoRoot, "tools", tool.Command); // tool.Command = "git_diff.ps1"
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException("Tool script not found.", scriptPath);

            // Run PowerShell
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            var output = process?.StandardOutput.ReadToEnd() ?? string.Empty;
            var error = process?.StandardError.ReadToEnd();
            process?.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
                Console.Error.WriteLine($"⚠️ {toolName} script error: {error}");

            return output.Trim();
        }
    }
}
