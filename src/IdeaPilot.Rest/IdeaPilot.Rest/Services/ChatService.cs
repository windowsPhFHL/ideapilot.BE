using Azure;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatMessage = IdeaPilot.Rest.Data.Models.ChatMessage;

namespace IdeaPilot.Rest.Services
{
    public class ChatService
    {
        private readonly OpenAIClient _openAiClient;
        private readonly Container _chatContainer;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _deploymentName = "gpt-4";
        private readonly string _containerName;
        private readonly IConfiguration _configuration; // Add this line

        public ChatService(IConfiguration configuration, CosmosClient cosmosClient, BlobServiceClient blobServiceClient)
        {
            _configuration = configuration; // Save IConfiguration into the private field

            // OpenAI API 🔑
            string openAiEndpoint = configuration["OpenAI:Endpoint"];
            string openAiKey = configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(openAiEndpoint) || string.IsNullOrEmpty(openAiKey))
            {
                throw new InvalidOperationException("OpenAI API Configuration is missing");
            }
            _openAiClient = new OpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey));

            // Cosmos DB 🔥
            string databaseId = configuration["CosmosDb:DatabaseId"];
            string containerId = configuration["CosmosDb:ContainerId"];
            _chatContainer = cosmosClient.GetContainer(databaseId, containerId);

            // Blob Storage 📄
            _blobServiceClient = blobServiceClient;
            _containerName = configuration["AzureBlobStorage:ContainerName"];
        }

        // Save Chat Message
        public async Task SaveChatMessageAsync(string sessionId, string user, string message)
        {
            var chatMessage = new ChatMessage(sessionId, user, message);
            await _chatContainer.CreateItemAsync(chatMessage, new PartitionKey(user));
        }

        // Get Chat History
        public async Task<List<ChatMessage>> GetChatHistoryAsync(string sessionId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.chatSessionId = @sessionId ORDER BY c.timestamp ASC")
                .WithParameter("@sessionId", sessionId);

            var iterator = _chatContainer.GetItemQueryIterator<ChatMessage>(query);
            List<ChatMessage> messages = new();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                messages.AddRange(response);
            }

            return messages;
        }

        // Get Chat Response from GPT-4o 🤖
        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            var options = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new Azure.AI.OpenAI.ChatMessage(ChatRole.User, userMessage)
                },
                MaxTokens = 500,
                Temperature = 0.7f
            };

            var response = await _openAiClient.GetChatCompletionsAsync(_deploymentName, options);
            return response.Value.Choices.First().Message.Content;
        }

        // Generate One Pager for the chat session
        public async Task<string> GenerateOnePagerAsync(string sessionId)
        {
            var chatHistory = await GetChatHistoryAsync(sessionId);
            string combinedChat = string.Join("\n", chatHistory.Select(m => $"{m.User}: {m.Message}"));

            var options = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new Azure.AI.OpenAI.ChatMessage(ChatRole.System, "You are an expert business analyst."),
                    new Azure.AI.OpenAI.ChatMessage(ChatRole.User, $"Generate a professional One Pager document:\n{combinedChat}")
                },
                MaxTokens = 1000,
                Temperature = 0.7f
            };

            var response = await _openAiClient.GetChatCompletionsAsync(_deploymentName, options);
            string onePagerContent = response.Value.Choices.First().Message.Content;

            // Generate PDF and upload to Blob Storage
            PdfService pdfService = new PdfService(_configuration);
            string pdfUrl = await pdfService.GeneratePdfAsync(onePagerContent, $"{sessionId}-OnePager-{DateTime.UtcNow.Ticks}.pdf");

            return pdfUrl;
        }
    }
}
