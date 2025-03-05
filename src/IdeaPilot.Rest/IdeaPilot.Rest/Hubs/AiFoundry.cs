using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;

namespace IdeaPilot.Rest.Hubs;

internal class AiFoundry
{
    static string GetEnvironmentVariable(string variable)
    {
        return Environment.GetEnvironmentVariable(variable);
    }

    public static async Task RunAsync(string user, string message)
    {
        // Retrieve the OpenAI endpoint from environment variables
        var endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://test-idea-pilot-ai-services.openai.azure.com/";
        if (string.IsNullOrEmpty(endpoint))
        {
            Console.WriteLine("Please set the AZURE_OPENAI_ENDPOINT environment variable.");
            return;
        }

        // Use DefaultAzureCredential for Entra ID authentication
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = "c03b638e-65c9-499b-954e-93ad15c08a29" });

        // Initialize the AzureOpenAIClient
        var azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);

        // Initialize the ChatClient with the specified deployment name
        ChatClient chatClient = azureClient.GetChatClient("gpt-4o-mini");

        // Create a list of chat messages
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an AI assistant that helps people find information."),
            new UserChatMessage("hey"),
        };

        // Create chat completion options  
        var options = new ChatCompletionOptions
        {
            Temperature = (float)0.7,
            MaxOutputTokenCount = 800,

            TopP = (float)0.95,
            FrequencyPenalty = (float)0,
            PresencePenalty = (float)0
        };

        try
        {
            // Create the chat completion request
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

            // Print the response
            if (completion != null)
            {
                Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine("No response received.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

}