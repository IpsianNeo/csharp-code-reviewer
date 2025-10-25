using Reviewer;
using Reviewer.Helpers;

using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Automatically find manifest
            var manifestPath = PathHelper.GetManifestPath();
            var orchestrator = new Orchestrator(manifestPath);

            // Detect branches
            string sourceBranch = Environment.GetEnvironmentVariable("GITHUB_HEAD_REF")
                                  ?? Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEBRANCH")
                                  ?? GetCurrentGitBranch();

            string targetBranch = Environment.GetEnvironmentVariable("GITHUB_BASE_REF")
                                  ?? Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_TARGETBRANCH")
                                  ?? "master"; // fallback if running locally

            Console.WriteLine($"🚀 Running review: '{sourceBranch}' -> '{targetBranch}'");

            // Run orchestrator
            orchestrator.RunReview(targetBranch, sourceBranch);

            Console.WriteLine("✅ Review completed successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("❌ Error during review: " + ex.Message);
        }
    }

    /// <summary>
    /// Gets the current Git branch when running locally.
    /// </summary>
    private static string GetCurrentGitBranch()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "rev-parse --abbrev-ref HEAD",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        string output = process?.StandardOutput.ReadToEnd().Trim() ?? string.Empty;
        process?.WaitForExit();

        if (string.IsNullOrEmpty(output))
            throw new Exception("Could not determine current Git branch.");

        return output;
    }
}
