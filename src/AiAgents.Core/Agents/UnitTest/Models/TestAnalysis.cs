namespace AiAgents.Core.Agents.UnitTest.Models
{
    public class TestAnalysis
    {
        public TestExecutionResult TestResult { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public List<string> Recommendations { get; set; } = [];
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int SkippedTests { get; set; }

        /// <summary>
        /// Detailed analysis from AI model (null if AI is not configured)
        /// </summary>
        public string? AiAnalysis { get; set; }
    }
}
