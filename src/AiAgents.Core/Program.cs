using AiAgents.Core.Agents.UnitTest;
using AiAgents.Core.Agents.UnitTest.Models;
using AiAgents.Core.Services;
using Microsoft.Extensions.Configuration;

namespace AiAgents.Core
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== AiAgents - AI-powered Unit Test Runner ===");
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
            bool skipAi = false;

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
                else if (args[i] == "--no-ai")
                {
                    skipAi = true;
                }
            }

            try
            {
                // Load AI configuration
                AiAnalysisService? aiService = null;

                if (!skipAi)
                {
                    aiService = TryCreateAiService();
                }

                Console.WriteLine($"Initializing UnitTestAgent for: {projectPath}");
                if (aiService != null)
                    Console.WriteLine("AI analysis: enabled");
                else
                    Console.WriteLine("AI analysis: disabled (configure appsettings.json or use --no-ai)");

                Console.WriteLine();

                var agent = new UnitTestAgent(projectPath, config, aiService);

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

                if (!string.IsNullOrWhiteSpace(analysis.AiAnalysis))
                {
                    Console.WriteLine();
                    Console.WriteLine("=== AI Analysis ===");
                    Console.WriteLine(analysis.AiAnalysis);
                }

                Environment.ExitCode = result.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        static AiAnalysisService? TryCreateAiService()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables("AIAGENTS_")
                    .Build();

                var aiSettings = new AiSettings();
                configuration.GetSection("AiSettings").Bind(aiSettings);

                // Validate that we have actual credentials configured
                if (string.Equals(aiSettings.Provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(aiSettings.AzureOpenAI.ApiKey) ||
                        aiSettings.AzureOpenAI.ApiKey.Contains("YOUR-"))
                        return null;
                }
                else if (string.Equals(aiSettings.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(aiSettings.OpenAI.ApiKey) ||
                        aiSettings.OpenAI.ApiKey.Contains("YOUR-"))
                        return null;
                }

                return new AiAnalysisService(aiSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Could not initialize AI service: {ex.Message}");
                return null;
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
            Console.WriteLine("  --no-ai                         Skip AI analysis");
            Console.WriteLine();
            Console.WriteLine("AI Configuration:");
            Console.WriteLine("  Configure AI settings in appsettings.json:");
            Console.WriteLine("    - Provider: 'AzureOpenAI' or 'OpenAI'");
            Console.WriteLine("    - AzureOpenAI: Endpoint, ApiKey, DeploymentName");
            Console.WriteLine("    - OpenAI: ApiKey, Model");
            Console.WriteLine();
            Console.WriteLine("  Or use environment variables (prefixed with AIAGENTS_):");
            Console.WriteLine("    - AIAGENTS_AiSettings__Provider");
            Console.WriteLine("    - AIAGENTS_AiSettings__AzureOpenAI__ApiKey");
            Console.WriteLine("    - AIAGENTS_AiSettings__OpenAI__ApiKey");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  AiAgents test C:\\Projects\\MyApp.Tests");
            Console.WriteLine("  AiAgents test C:\\Projects\\MyApp.Tests\\MyApp.Tests.csproj");
            Console.WriteLine("  AiAgents test /path/to/project -c Release");
            Console.WriteLine("  AiAgents test /path/to/project --no-ai");
        }
    }
}
