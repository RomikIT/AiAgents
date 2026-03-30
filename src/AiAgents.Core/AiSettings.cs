namespace AiAgents.Core
{
    public class AiSettings
    {
        /// <summary>
        /// Provider: "AzureOpenAI" or "OpenAI"
        /// </summary>
        public string Provider { get; set; } = "AzureOpenAI";

        public AzureOpenAISettings AzureOpenAI { get; set; } = new();
        public OpenAISettings OpenAI { get; set; } = new();
    }

    public class AzureOpenAISettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string DeploymentName { get; set; } = "gpt-4o";
    }

    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o";
    }
}
