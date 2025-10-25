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
            // allow manifestPath optional: use PathHelper if null
            var manifestFile = manifestPath;
            if (string.IsNullOrWhiteSpace(manifestFile))
                manifestFile = PathHelper.GetManifestPath();

            if (!File.Exists(manifestFile))
                throw new FileNotFoundException("MCP manifest not found.", manifestFile);

            var yaml = File.ReadAllText(manifestFile);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            _manifest = deserializer.Deserialize<McpManifest>(yaml) ?? new McpManifest();

            ValidateManifest();
        }

        private void ValidateManifest()
        {
            if (_manifest.Tools == null || _manifest.Tools.Count == 0)
                throw new InvalidOperationException("MCP manifest contains no tools.");

            foreach (var kv in _manifest.Tools)
            {
                var name = kv.Key;
                var tool = kv.Value;
                if (string.IsNullOrWhiteSpace(tool.Args) && string.IsNullOrWhiteSpace(tool.Command))
                    throw new InvalidOperationException($"Tool '{name}' must define either 'args' or 'command' in the manifest.");
            }
        }

        /// <summary>
        /// Run a tool specified in the manifest. Parameters are substituted into the tool's args string.
        /// Returns stdout trimmed. Throws for fatal errors (tool missing, process start failure, timeout).
        /// </summary>
        public string RunTool(string toolName, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrWhiteSpace(toolName))
                throw new ArgumentNullException(nameof(toolName));

            if (!_manifest.Tools.ContainsKey(toolName))
                throw new KeyNotFoundException($"Tool '{toolName}' not found in MCP manifest.");

            var tool = _manifest.Tools[toolName];

            // Prepare argument string with parameter substitution
            var argsTemplate = tool.Args ?? string.Empty;
            foreach (var kv in parameters ?? new Dictionary<string, string>())
            {
                argsTemplate = argsTemplate.Replace("{" + kv.Key + "}", kv.Value ?? string.Empty);
            }

            // Find repo root (contains tools/mcp_manifest.yml)
            var repoRoot = PathHelper.FindRepoRootWithTools();
            if (repoRoot == null)
                throw new DirectoryNotFoundException("Repository root containing 'tools/mcp_manifest.yml' not found.");

            // Decide which shell/command to run
            string shellProgram = tool.Shell;
            string shellArgs = argsTemplate;

            // If manifest used 'command' instead of shell+args, allow running the script directly by wrapping in shell
            if (string.IsNullOrWhiteSpace(shellProgram))
            {
                // fallback: if args reference a tools/... path and on Windows use powershell, otherwise try 'bash'
                // but prefer explicit shell in manifest. Default to powershell on Windows.
                shellProgram = OperatingSystem.IsWindows() ? "powershell" : "bash";
            }

            // Build ProcessStartInfo
            var psi = new ProcessStartInfo
            {
                FileName = shellProgram,
                Arguments = shellArgs,
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start process
            using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start tool process '{shellProgram}' for '{toolName}'.");

            // Timeout handling
            var timeoutMs = (tool.Timeout_Seconds > 0) ? tool.Timeout_Seconds * 1000 : 60_000; // default 60s
            var exited = process.WaitForExit(timeoutMs);
            if (!exited)
            {
                try { process.Kill(true); } catch { /* ignore */ }
                throw new TimeoutException($"Tool '{toolName}' timed out after {timeoutMs / 1000.0} seconds.");
            }

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();

            // Log stderr as warning (do not throw) to allow pipeline to continue; but surface it
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                // Write to Console.Error so CI shows it clearly
                Console.Error.WriteLine($"[MCP:{toolName}] stderr: {stderr.Trim()}");
            }

            return stdout?.Trim() ?? string.Empty;
        }
    }
}
