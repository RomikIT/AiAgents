using AiAgents.Core.Agents;

namespace AiAgents.Core
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== AiAgents - Unit Test Runner ===");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "test":
                    await RunTestCommand(args);
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowUsage();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowUsage();
                    break;
            }
        }

        static async Task RunTestCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Project path is required");
                Console.WriteLine("Usage: AiAgents test <project-path> [options]");
                return;
            }

            var projectPath = args[1];

            // Parse configuration options
            var config = new AgentConfiguration();
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--configuration" || args[i] == "-c")
                {
                    if (i + 1 < args.Length)
                        config.Configuration = args[++i];
                }
                else if (args[i] == "--verbosity" || args[i] == "-v")
                {
                    if (i + 1 < args.Length)
                        config.Verbosity = args[++i];
                }
                else if (args[i] == "--filter" || args[i] == "-f")
                {
                    if (i + 1 < args.Length)
                        config.Filter = args[++i];
                }
                else if (args[i] == "--no-build")
                {
                    config.NoBuild = true;
                }
                else if (args[i] == "--no-restore")
                {
                    config.NoRestore = true;
                }
            }

            try
            {
                Console.WriteLine($"Initializing UnitTestAgent for: {projectPath}");
                var agent = new UnitTestAgent(projectPath, config);

                var result = await agent.RunTestsAsync();

                Console.WriteLine();
                Console.WriteLine("=== Test Execution Results ===");
                Console.WriteLine($"Duration: {result.Duration.TotalSeconds:F2} seconds");
                Console.WriteLine($"Success: {result.Success}");
                Console.WriteLine($"Exit Code: {result.ExitCode}");

                // Analyze results
                var analysis = await agent.AnalyzeTestResultsAsync(result);

                Console.WriteLine();
                Console.WriteLine("=== Analysis ===");
                Console.WriteLine($"Total Tests: {analysis.TotalTests}");
                Console.WriteLine($"Passed: {analysis.PassedTests}");
                Console.WriteLine($"Failed: {analysis.FailedTests}");
                Console.WriteLine($"Skipped: {analysis.SkippedTests}");

                if (analysis.Recommendations.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("=== Recommendations ===");
                    foreach (var recommendation in analysis.Recommendations)
                    {
                        Console.WriteLine($"- {recommendation}");
                    }
                }

                Environment.ExitCode = result.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("AiAgents - AI-powered unit test runner");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  AiAgents test <project-path> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  test              Run unit tests for the specified project");
            Console.WriteLine("  help              Show this help message");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -c, --configuration <config>    Build configuration (Debug/Release)");
            Console.WriteLine("  -v, --verbosity <level>         Test output verbosity");
            Console.WriteLine("  -f, --filter <expression>       Filter tests to run");
            Console.WriteLine("  --no-build                      Skip building the project");
            Console.WriteLine("  --no-restore                    Skip restoring dependencies");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  AiAgents test /path/to/project");
            Console.WriteLine("  AiAgents test /path/to/project -c Release");
            Console.WriteLine("  AiAgents test /path/to/project -f \"Category=Unit\"");
        }
    }
}
