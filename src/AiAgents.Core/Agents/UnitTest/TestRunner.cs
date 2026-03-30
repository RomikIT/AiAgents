using System.Diagnostics;
using System.Text;
using AiAgents.Core.Agents.UnitTest.Models;

namespace AiAgents.Core.Agents.UnitTest
{
    /// <summary>
    /// Discovers test projects and executes dotnet test.
    /// </summary>
    public class TestRunner
    {
        private readonly AgentConfiguration _config;

        public TestRunner(AgentConfiguration config)
        {
            _config = config;
        }

        public async Task<TestExecutionResult> RunAsync(string projectPath)
        {
            var result = new TestExecutionResult
            {
                ProjectPath = projectPath,
                StartTime = DateTime.UtcNow
            };

            try
            {
                var targetPath = ResolveProjectPath(projectPath);

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = BuildArguments(targetPath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };

                var output = new StringBuilder();
                var error = new StringBuilder();

                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                        Console.WriteLine($"[UnitTestAgent] {e.Data}");
                    }
                };

                process.ErrorDataReceived += (_, e) =>
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
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                Console.WriteLine($"[UnitTestAgent] Error during test execution: {ex.Message}");
            }
            finally
            {
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
            }

            return result;
        }

        private static string ResolveProjectPath(string path)
        {
            if (!Directory.Exists(path))
                return path;

            var testProjects = Directory.GetFiles(path, "*Test*.csproj", SearchOption.AllDirectories);
            if (testProjects.Length == 0)
                testProjects = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);

            if (testProjects.Length > 0)
            {
                Console.WriteLine($"[UnitTestAgent] Found test project: {testProjects[0]}");
                return testProjects[0];
            }

            return path;
        }

        private string BuildArguments(string targetPath)
        {
            var args = new StringBuilder($"test \"{targetPath}\"");

            args.Append(" --logger \"console;verbosity=normal\"");

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
}
