using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Storage.Blobs;
using DocumentFormat.OpenXml.Spreadsheet;
using IdeaPilot.Rest.Configuration;
using IdeaPilot.Rest.Data.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using IdeaPilot.Rest.Hubs;

namespace IdeaPilot.Rest.Services
{
    public class ChatService
    {
        private readonly OpenAIClient _openAiClient;
        private readonly Container _chatContainer;
        private readonly string _deploymentName;
        private readonly string _model;
        private readonly float _temperature;
        private readonly ICosmosDbRepository<Message> _chatRepository;
        private readonly int _maxTokens; // Set the max tokens for the chat completion
        private readonly OpenAIClient _openAIClient;

        //add blob storage client and blob ioptions
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _blobContainerName;

        public ChatService(
            OpenAIClient openAIClient,
            BlobServiceClient blobServiceClient, 
            ICosmosDbRepository<Message> iCosmosRepo,
            IOptions<OpenAIOptions> openAIOptions,
            IOptions<CosmosDbOptions> cosmosDbOptions
            )
        {
            _openAiClient = openAIClient;
            _blobServiceClient = blobServiceClient;
            _chatRepository = iCosmosRepo;

            // Cosmos DB 🔥
            string databaseId = cosmosDbOptions.Value.DatabaseId;
            string containerId = cosmosDbOptions.Value.ContainerId;

            _deploymentName = openAIOptions.Value.DeploymentName;
            _model = openAIOptions.Value.Model;
            _temperature = openAIOptions.Value.Temperature;
            _maxTokens = openAIOptions.Value.MaxTokens; // Set the max tokens for the chat completion

            // Blob Storage
            //_blobContainerName = blobStorageOptions.Value.ContainerName;

        }

        //create a method to connect open ai
        public async Task<string> ProcessModelMessages(Message message)
        {
            
            //get list of messages from the database
            Dictionary<string, string> props = new Dictionary<string, string>();

            props.Add("ContainerType", "Message");
            props.Add("WorkspaceId", message.WorkspaceId);

            var items = _chatRepository.ListItemsAsync(props);

            if (items == null || !items.Result.Any())
            {
                throw new InvalidOperationException("No chat history found for the user.");
            }

            var options = new ChatCompletionsOptions();
            options.MaxTokens = _maxTokens;
            options.Temperature = _temperature;

            //loop through chat items and log them
            foreach (var item in items.Result)
            {
                options.Messages.Add(new Azure.AI.OpenAI.ChatMessage(ChatRole.User, item.Text));
            }

            var response = await _openAiClient.GetChatCompletionsAsync(_deploymentName, options);

            //get the response content
            var responseContent = response.Value.Choices.First().Message.Content;

            var modelMessage = new Message
            {
                UserId = "assistant",
                ChatId = message.ChatId,
                WorkspaceId = message.WorkspaceId,
                Text = responseContent,
                Status = "Success"
            };
            //log response to cosmos db
            await _chatRepository.CreateItemAsync(modelMessage, modelMessage.id);

            //check if response is of json type
            if (responseContent.StartsWith("{") && responseContent.EndsWith("}"))
            {
                //parse the json response
                var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);


                //check if the json response has a key called "type"
                if (jsonResponse.ContainsKey("type"))
                {
                    ////insert  the content to blob storage

                }
                //return the json response
                //return Newtonsoft.Json.JsonConvert.SerializeObject(jsonResponse);
            }

            return responseContent;
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
        public async Task<string> GetChatResponseAsync(Message message)
        {

            Dictionary<string, string> props = new Dictionary<string, string>();

            props.Add("ContainerType", "Message");


            var items = _chatRepository.ListItemsAsync(props);

            if (items == null || !items.Result.Any())
            {
                throw new InvalidOperationException("No chat history found for the user.");
            }

            var options = new ChatCompletionsOptions();
            options.MaxTokens = 4096;
            options.Temperature = 1.1f;

            //loop through chat items and log them
            foreach (var item in items.Result)
            {
                System.Console.WriteLine($"User: {item.UserId}, Message: {item.Text}");
                options.Messages.Add(new Azure.AI.OpenAI.ChatMessage(ChatRole.User, item.Text));
            }

            var response = await _openAiClient.GetChatCompletionsAsync(_deploymentName, options);

            var modelMessage = new Message
            {
                UserId = "GPT-4o",
                ChatId = message.ChatId,
                WorkspaceId = message.WorkspaceId,
                Text = response.Value.Choices.First().Message.Content,
                Status = "Success"
            };
            //log response to cosmos db
            await _chatRepository.CreateItemAsync(modelMessage, modelMessage.id);
            return response.Value.Choices.First().Message.Content;
        }

        // Generate One Pager for the chat session
        public async Task<string> GenerateOnePagerAsync(string sessionId)
        {
            return "One Pager generation is not implemented yet.";
            //var chatHistory = await GetChatHistoryAsync(sessionId);
            //string combinedChat = string.Join("\n", chatHistory.Select(m => $"{m.User}: {m.Message}"));

            //var options = new ChatCompletionsOptions()
            //{
            //    Messages =
            //    {
            //        new Azure.AI.OpenAI.ChatMessage(ChatRole.System, "You are an expert business analyst."),
            //        new Azure.AI.OpenAI.ChatMessage(ChatRole.User, $"Generate a professional One Pager document:\n{combinedChat}")
            //    },
            //    MaxTokens = 1000,
            //    Temperature = 0.7f
            //};

            //var response = await _openAiClient.GetChatCompletionsAsync(_deploymentName, options);
            //string onePagerContent = response.Value.Choices.First().Message.Content;

            //// Generate PDF and upload to Blob Storage
            //PdfService pdfService = new PdfService(_configuration);
            //string pdfUrl = await pdfService.GeneratePdfAsync(onePagerContent, $"{sessionId}-OnePager-{DateTime.UtcNow.Ticks}.pdf");

            //return pdfUrl;
        }
    }
}
