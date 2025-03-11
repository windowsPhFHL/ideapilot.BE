using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Collections.Concurrent;
using Azure.Data.Tables;

namespace IdeaPilot.Rest.Hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentQueue<string> ChatHistory = new();
        private readonly OpenAIClient _client;
        private readonly string _deploymentName;
        private readonly TableClient _tableClient;

        public ChatHub(IConfiguration config)
        {
            var endpoint = config["OpenAI:Endpoint"];
            var apiKey = config["OpenAI:ApiKey"];
            _deploymentName = "gpt-4";
            _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            string storageConnectionString = config["Azure:StorageConnectionString"];
            string tableName = "ChatHistory";
            _tableClient = new TableClient(storageConnectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task SendMessage(string user, string message)
        {
            ChatHistory.Enqueue($"{user}: {message}");
            await SaveChatToCosmosDB(user, message);

            // Call GPT for response
            var response = await GetChatResponse(message);
            await Clients.All.SendAsync("ReceiveMessage", user, response);

            // Check for One-Pager Trigger
            if (message.Contains("one-pager", StringComparison.OrdinalIgnoreCase))
            {
                string fullChat = string.Join("\n", ChatHistory);
                var onePager = await GenerateOnePager(fullChat);
                await Clients.All.SendAsync("ReceiveMessage", "System", $"One-Pager Generated ✅:\n{onePager}");
            }
        }

        private async Task SaveChatToCosmosDB(string user, string message)
        {
            var entity = new TableEntity(Guid.NewGuid().ToString(), DateTime.UtcNow.Ticks.ToString())
            {
                { "User", user },
                { "Message", message },
                { "Timestamp", DateTime.UtcNow.ToString() }
            };
            await _tableClient.AddEntityAsync(entity);
        }

        private async Task<string> GetChatResponse(string message)
        {
            var options = new ChatCompletionsOptions()
            {
                Messages = { new ChatMessage(ChatRole.User, message) },
                MaxTokens = 100,
                Temperature = 0.7f
            };
            var result = await _client.GetChatCompletionsAsync(_deploymentName, options);
            return result.Value.Choices[0].Message.Content;
        }

        private async Task<string> GenerateOnePager(string chatHistory)
        {
            var options = new ChatCompletionsOptions()
            {
                Messages = { new ChatMessage(ChatRole.User, $"Generate a one-pager based on the following chat history:\n{chatHistory}") },
                MaxTokens = 1024,
                Temperature = 0.7f
            };
            var result = await _client.GetChatCompletionsAsync(_deploymentName, options);
            return result.Value.Choices[0].Message.Content;
        }
    }
}
