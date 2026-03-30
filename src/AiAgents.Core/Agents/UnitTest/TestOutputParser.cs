using System.Text.RegularExpressions;
using AiAgents.Core.Agents.UnitTest.Models;

namespace AiAgents.Core.Agents.UnitTest
{
    /// <summary>
    /// Parses dotnet test output to extract test statistics.
    /// </summary>
    public static class TestOutputParser
    {
        public static void PopulateStatistics(TestAnalysis analysis, string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return;

            // Pattern: "Total tests: 42"
            var totalMatch = Regex.Match(output, @"Total\s+tests?:\s*(\d+)", RegexOptions.IgnoreCase);
            if (totalMatch.Success)
                analysis.TotalTests = int.Parse(totalMatch.Groups[1].Value);

            // Pattern: "Passed: 40"
            var passedMatch = Regex.Match(output, @"Passed\s*:\s*(\d+)", RegexOptions.IgnoreCase);
            if (passedMatch.Success)
                analysis.PassedTests = int.Parse(passedMatch.Groups[1].Value);

            // Pattern: "Failed: 2"
            var failedMatch = Regex.Match(output, @"Failed\s*:\s*(\d+)", RegexOptions.IgnoreCase);
            if (failedMatch.Success)
                analysis.FailedTests = int.Parse(failedMatch.Groups[1].Value);

            // Pattern: "Skipped: 1"
            var skippedMatch = Regex.Match(output, @"Skipped\s*:\s*(\d+)", RegexOptions.IgnoreCase);
            if (skippedMatch.Success)
                analysis.SkippedTests = int.Parse(skippedMatch.Groups[1].Value);

            if (analysis.TotalTests == 0 && (analysis.PassedTests > 0 || analysis.FailedTests > 0 || analysis.SkippedTests > 0))
                analysis.TotalTests = analysis.PassedTests + analysis.FailedTests + analysis.SkippedTests;

            // Fallback: count individual test result lines like "Passed MethodName"
            if (analysis.TotalTests == 0)
            {
                var passedCount = Regex.Matches(output, @"^\s*Passed\s+\S+", RegexOptions.Multiline | RegexOptions.IgnoreCase).Count;
                var failedCount = Regex.Matches(output, @"^\s*Failed\s+\S+", RegexOptions.Multiline | RegexOptions.IgnoreCase).Count;
                var skippedCount = Regex.Matches(output, @"^\s*Skipped\s+\S+", RegexOptions.Multiline | RegexOptions.IgnoreCase).Count;

                if (passedCount > 0 || failedCount > 0 || skippedCount > 0)
                {
                    analysis.PassedTests = passedCount;
                    analysis.FailedTests = failedCount;
                    analysis.SkippedTests = skippedCount;
                    analysis.TotalTests = passedCount + failedCount + skippedCount;
                }
            }
        }
    }
}
