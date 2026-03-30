using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AiAgents.Core.Services
{
    public class AiAnalysisService
    {
        private readonly ChatClient _chatClient;

        public AiAnalysisService(AiSettings settings)
        {
            if (string.Equals(settings.Provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(settings.AzureOpenAI.Endpoint))
                    throw new InvalidOperationException("AzureOpenAI Endpoint is not configured. Set it in appsettings.json or environment variables.");
                if (string.IsNullOrWhiteSpace(settings.AzureOpenAI.ApiKey))
                    throw new InvalidOperationException("AzureOpenAI ApiKey is not configured. Set it in appsettings.json or environment variables.");

                var azureClient = new AzureOpenAIClient(
                    new Uri(settings.AzureOpenAI.Endpoint),
                    new ApiKeyCredential(settings.AzureOpenAI.ApiKey));

                _chatClient = azureClient.GetChatClient(settings.AzureOpenAI.DeploymentName);
            }
            else if (string.Equals(settings.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(settings.OpenAI.ApiKey))
                    throw new InvalidOperationException("OpenAI ApiKey is not configured. Set it in appsettings.json or environment variables.");

                var openAiClient = new OpenAI.OpenAIClient(new ApiKeyCredential(settings.OpenAI.ApiKey));
                _chatClient = openAiClient.GetChatClient(settings.OpenAI.Model);
            }
            else
            {
                throw new InvalidOperationException($"Unknown AI provider: '{settings.Provider}'. Use 'AzureOpenAI' or 'OpenAI'.");
            }
        }

        /// <summary>
        /// Sends test output to AI for analysis and returns recommendations.
        /// </summary>
        public async Task<string> AnalyzeTestOutputAsync(string testOutput, string errorOutput)
        {
            var systemPrompt = """
                You are an expert .NET developer and test analyst. 
                Analyze the unit test execution results provided below and give:
                1. A summary of what happened (passed/failed/skipped counts).
                2. For each failing test — the root cause and a suggested fix.
                3. General recommendations to improve test quality.
                Respond in a clear, concise manner.
                """;

            var userMessage = $"""
                === Test Standard Output ===
                {Truncate(testOutput, 6000)}

                === Test Error Output ===
                {Truncate(errorOutput, 2000)}
                """;

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage)
            };

            Console.WriteLine("[AI] Sending test results to AI for analysis...");

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);

            return completion.Content[0].Text;
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text[..maxLength] + "\n... [truncated]";
        }
    }
}
