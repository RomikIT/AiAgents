using AiAgents.Core.Agents.UnitTest.Models;
using AiAgents.Core.Services;

namespace AiAgents.Core.Agents.UnitTest
{
    /// <summary>
    /// Analyzes test execution results using basic heuristics or AI.
    /// </summary>
    public class TestAnalyzer
    {
        private readonly AiAnalysisService? _aiService;

        public TestAnalyzer(AiAnalysisService? aiService = null)
        {
            _aiService = aiService;
        }

        public async Task<TestAnalysis> AnalyzeAsync(TestExecutionResult result)
        {
            var analysis = new TestAnalysis
            {
                TestResult = result,
                Timestamp = DateTime.UtcNow
            };

            var combinedOutput = result.Output + "\n" + result.Error;
            TestOutputParser.PopulateStatistics(analysis, combinedOutput);

            if (_aiService != null)
            {
                await TryAiAnalysis(analysis, result);
            }
            else
            {
                AddBasicRecommendations(analysis, result);
            }

            return analysis;
        }

        private async Task TryAiAnalysis(TestAnalysis analysis, TestExecutionResult result)
        {
            try
            {
                var aiAnalysis = await _aiService!.AnalyzeTestOutputAsync(result.Output, result.Error);
                analysis.AiAnalysis = aiAnalysis;
                analysis.Recommendations.Add("See AI Analysis below for detailed recommendations.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UnitTestAgent] AI analysis failed: {ex.Message}");
                analysis.Recommendations.Add($"AI analysis unavailable: {ex.Message}");
                AddBasicRecommendations(analysis, result);
            }
        }

        private static void AddBasicRecommendations(TestAnalysis analysis, TestExecutionResult result)
        {
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
        }
    }
}
