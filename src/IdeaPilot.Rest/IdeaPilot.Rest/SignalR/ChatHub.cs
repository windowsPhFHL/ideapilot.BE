using IdeaPilot.Rest.Data.Entities;
using IdeaPilot.Rest.Hubs;
using Microsoft.AspNetCore.SignalR;
using Azure.AI.OpenAI;

namespace IdeaPilot.Rest.SignalR;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly ICosmosDbRepository<Message> _cosmosDbRepository;
    private readonly OpenAIClient _openAIClient;
    private readonly string _deploymentName;

    public ChatHub(
        OpenAIClient openAIClient,
        ILogger<ChatHub> logger,
        ICosmosDbRepository<Message> cosmosDbRepository)
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _cosmosDbRepository = cosmosDbRepository;
        _deploymentName = "gpt-4"; // Could be moved to configuration
    }

    public async Task SendMessage(string user, string message, string chatId)
    {
        // Log the message
        _logger.LogInformation($"Received message from {user}: {message}");

        //create a new message from the message
        var newMessage = new Message
        {
            UserId = user,
            Text = message,
            ChatId = chatId,
        };

        // Save the message to Cosmos DB
        await _cosmosDbRepository.CreateItemAsync(newMessage, newMessage.UserId.ToString());
        await Clients.All.SendAsync($"Received message from {user} :  {message}");
    }

    // Method to get AI response to a message
    public async Task<string> GetAIResponse(string message)
    {
        try
        {
            var options = new ChatCompletionsOptions
            {
                Messages = {
                    new ChatMessage(ChatRole.System, "You are an AI assistant that helps people find information."),
                    new ChatMessage(ChatRole.User, message)
                },
                MaxTokens = 800,
                Temperature = 0.7f,
                //TopP = 0.95f
            };

            var response = await _openAIClient.GetChatCompletionsAsync(_deploymentName, options);
            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI response");
            return "Sorry, I encountered an error processing your request.";
        }
    }

    // Method to get all messges from the Cosmos DB
    public async Task GetAllMessages()
    {
        // Get all messages from the Cosmos DB
        var messages = await _cosmosDbRepository.ListItemsAsync();
        await Clients.All.SendAsync("GetAllMessages", messages);
    }

    public override async Task OnConnectedAsync()
    {
        //await Clients.Caller.SendAsync("ReceiveConnectionId", Context.ConnectionId);
        //await Clients.Caller.SendAsync("GetAllConversations");
        await base.OnConnectedAsync();
    }

    // get specific chat messages using chatId
    public async Task GetChatMessages(string chatId)
    {
        // Get all messages from the Cosmos DB
        var messages = await _cosmosDbRepository.ListItemsAsync("ChatId", chatId);

        // Sort messages by CreatedOn from earliest to latest
        var sortedMessages = messages.OrderBy(m => m.CreatedOn);

        Console.WriteLine(sortedMessages);
        await Clients.Caller.SendAsync("GetChatMessages", sortedMessages);
    }

    // get specific chat messages using chatId
    public async Task GetWorkspaceMessages(string chatId)
    {
        // Get all messages from the Cosmos DB
        var messages = await _cosmosDbRepository.ListItemsAsync("WorkspaceId", chatId);
        // Sort messages by CreatedOn from earliest to latest
        var sortedMessages = messages.OrderBy(m => m.CreatedOn);
        Console.WriteLine(sortedMessages);
        await Clients.Caller.SendAsync("GetWorkspaceMessages", sortedMessages);
    }
}
