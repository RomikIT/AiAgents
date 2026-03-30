namespace AiAgents.Core.Agents.UnitTest.Models
{
    public class AgentConfiguration
    {
        public string? Verbosity { get; set; } = "normal";
        public bool NoRestore { get; set; } = false;
        public bool NoBuild { get; set; } = false;
        public string? Configuration { get; set; } = "Debug";
        public string? Framework { get; set; }
        public string? Filter { get; set; }
    }
}
