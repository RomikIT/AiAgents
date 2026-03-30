using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AiAgents.Core.Agents
{
    /// <summary>
    /// AI Agent responsible for running unit tests in other projects on a given path
    /// </summary>
    public class UnitTestAgent
    {
        private readonly string _projectPath;
        private readonly AgentConfiguration _config;

        public UnitTestAgent(string projectPath, AgentConfiguration? config = null)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("Project path cannot be null or empty", nameof(projectPath));

            if (!Directory.Exists(projectPath) && !File.Exists(projectPath))
                throw new DirectoryNotFoundException($"Path not found: {projectPath}");

            _projectPath = projectPath;
            _config = config ?? new AgentConfiguration();
        }

        /// <summary>
        /// Executes unit tests for the specified project
        /// </summary>
        /// <returns>Test execution results</returns>
        public async Task<TestExecutionResult> RunTestsAsync()
        {
            Console.WriteLine($"[UnitTestAgent] Starting test execution for: {_projectPath}");

            var result = new TestExecutionResult
            {
                ProjectPath = _projectPath,
                StartTime = DateTime.UtcNow
            };

            try
            {
                // Determine if path is a project file or directory
                var targetPath = _projectPath;
                if (Directory.Exists(_projectPath))
                {
                    // Find test project in directory
                    var testProjects = Directory.GetFiles(_projectPath, "*Test*.csproj", SearchOption.AllDirectories);
                    if (testProjects.Length == 0)
                    {
                        testProjects = Directory.GetFiles(_projectPath, "*.csproj", SearchOption.AllDirectories);
                    }

                    if (testProjects.Length > 0)
                    {
                        targetPath = testProjects[0];
                        Console.WriteLine($"[UnitTestAgent] Found test project: {targetPath}");
                    }
                }

                // Execute dotnet test command
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = BuildTestArguments(targetPath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    var output = new StringBuilder();
                    var error = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            output.AppendLine(e.Data);
                            Console.WriteLine($"[UnitTestAgent] {e.Data}");
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            error.AppendLine(e.Data);
                            Console.WriteLine($"[UnitTestAgent ERROR] {e.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync();

                    result.Output = output.ToString();
                    result.Error = error.ToString();
                    result.ExitCode = process.ExitCode;
                    result.Success = process.ExitCode == 0;
                }

                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;

                Console.WriteLine($"[UnitTestAgent] Test execution completed. Success: {result.Success}");

                return result;
            }
            catch (Exception ex)
            {
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.Success = false;
                result.Error = ex.Message;
                Console.WriteLine($"[UnitTestAgent] Error during test execution: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Analyzes test results and provides recommendations
        /// </summary>
        public async Task<TestAnalysis> AnalyzeTestResultsAsync(TestExecutionResult result)
        {
            var analysis = new TestAnalysis
            {
                TestResult = result,
                Timestamp = DateTime.UtcNow
            };

            if (!result.Success)
            {
                analysis.Recommendations.Add("Tests failed. Review error messages and fix failing tests.");

                if (result.Output.Contains("Build FAILED", StringComparison.OrdinalIgnoreCase))
                {
                    analysis.Recommendations.Add("Build errors detected. Fix compilation issues before running tests.");
                }
            }
            else
            {
                analysis.Recommendations.Add("All tests passed successfully!");
            }

            // Extract test statistics from output
            analysis.ExtractStatistics(result.Output);

            return analysis;
        }

        private string BuildTestArguments(string targetPath)
        {
            var args = new StringBuilder($"test \"{targetPath}\"");

            if (_config.Verbosity != null)
                args.Append($" --verbosity {_config.Verbosity}");

            if (_config.NoRestore)
                args.Append(" --no-restore");

            if (_config.NoBuild)
                args.Append(" --no-build");

            if (!string.IsNullOrWhiteSpace(_config.Configuration))
                args.Append($" --configuration {_config.Configuration}");

            if (!string.IsNullOrWhiteSpace(_config.Framework))
                args.Append($" --framework {_config.Framework}");

            if (!string.IsNullOrWhiteSpace(_config.Filter))
                args.Append($" --filter \"{_config.Filter}\"");

            return args.ToString();
        }
    }

    public class AgentConfiguration
    {
        public string? Verbosity { get; set; } = "normal";
        public bool NoRestore { get; set; } = false;
        public bool NoBuild { get; set; } = false;
        public string? Configuration { get; set; } = "Debug";
        public string? Framework { get; set; }
        public string? Filter { get; set; }
    }

    public class TestExecutionResult
    {
        public string ProjectPath { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class TestAnalysis
    {
        public TestExecutionResult TestResult { get; set; } = new TestExecutionResult();
        public DateTime Timestamp { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int SkippedTests { get; set; }

        public void ExtractStatistics(string output)
        {
            // Simple parsing of test output - can be enhanced
            if (output.Contains("Passed!"))
            {
                // Try to extract test counts from output
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("Passed:"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1 && int.TryParse(parts[1].Trim().Split(' ')[0], out int passed))
                        {
                            PassedTests = passed;
                        }
                    }
                    else if (line.Contains("Failed:"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1 && int.TryParse(parts[1].Trim().Split(' ')[0], out int failed))
                        {
                            FailedTests = failed;
                        }
                    }
                    else if (line.Contains("Skipped:"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1 && int.TryParse(parts[1].Trim().Split(' ')[0], out int skipped))
                        {
                            SkippedTests = skipped;
                        }
                    }
                }
                TotalTests = PassedTests + FailedTests + SkippedTests;
            }
        }
    }
}
