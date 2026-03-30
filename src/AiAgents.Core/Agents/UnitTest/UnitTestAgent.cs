using AiAgents.Core.Agents.UnitTest.Models;
using AiAgents.Core.Services;

namespace AiAgents.Core.Agents.UnitTest
{
    /// <summary>
    /// AI Agent responsible for running unit tests in other projects on a given path.
    /// Orchestrates test execution and result analysis.
    /// </summary>
    public class UnitTestAgent
    {
        private readonly TestRunner _runner;
        private readonly TestAnalyzer _analyzer;
        private readonly string _projectPath;

        public UnitTestAgent(string projectPath, AgentConfiguration? config = null, AiAnalysisService? aiService = null)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("Project path cannot be null or empty", nameof(projectPath));

            if (!Directory.Exists(projectPath) && !File.Exists(projectPath))
                throw new DirectoryNotFoundException($"Path not found: {projectPath}");

            _projectPath = projectPath;
            _runner = new TestRunner(config ?? new AgentConfiguration());
            _analyzer = new TestAnalyzer(aiService);
        }

        /// <summary>
        /// Executes unit tests for the specified project.
        /// </summary>
        public async Task<TestExecutionResult> RunTestsAsync()
        {
            Console.WriteLine($"[UnitTestAgent] Starting test execution for: {_projectPath}");

            var result = await _runner.RunAsync(_projectPath);

            Console.WriteLine($"[UnitTestAgent] Test execution completed. Success: {result.Success}");
            return result;
        }

        /// <summary>
        /// Analyzes test results and provides recommendations.
        /// </summary>
        public async Task<TestAnalysis> AnalyzeTestResultsAsync(TestExecutionResult result)
        {
            return await _analyzer.AnalyzeAsync(result);
        }
    }
}
