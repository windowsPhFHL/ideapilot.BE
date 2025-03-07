using IdeaPilot.Rest.Data.Entities;
using IdeaPilot.Rest.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using System;

namespace IdeaPilot.Rest.SignalR;

public class ChatHub : Hub
{

    //create a construsctor that takes a Kernel and a CosmosDbClient
    private readonly ILogger<ChatHub> _logger;
    private readonly ICosmosDbRepository<Message> _cosmosDbRepository;

    //add kernel to the constructor
    private readonly Kernel _kernel;
    public ChatHub(Kernel kernel, ILogger<ChatHub> logger, ICosmosDbRepository<Message> cosmosDbRepository)
    {
        _kernel = kernel;
        _logger = logger;
        _cosmosDbRepository = cosmosDbRepository;
    }

    public async Task SendMessage(Message message)
    {
        // Log the message
        _logger.LogInformation($"Received message from {message.UserId}: {message.Text}");

        //createa new message from the message
        var newMessage = new Message
        {
            UserId = message.UserId,
            Text = message.Text,
            ChatId = message.ChatId,
            Status = "Active"
        };

        // Save the message to Cosmos DB
        await _cosmosDbRepository.CreateItemAsync(newMessage, newMessage.UserId.ToString());



        //AiFoundry.RunAsync(user, message).Wait();

        //CosmosDbClient cosmosDbClient = new CosmosDbClient();
        // Save the message to Cosmos DB
        //await cosmosDbClient.SaveMessageAsync(user, message, Guid.NewGuid().ToString());

        //var func = _kernel.CreateFunctionFromPrompt(string.Empty);
        //_kernel.InvokeAsync(func);


        await Clients.All.SendAsync($"Received message from {message.UserId}: {message.Text}");
    }
    // Method to get all messges from the Cosmos DB
    public async Task GetAllMessages()
    {
        // Get all messages from the Cosmos DB
        var messages = await _cosmosDbRepository.ListItemsAsync();
        await Clients.All.SendAsync("GetMessages", messages);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("GetConnectionId", Context.ConnectionId);
        //await Clients.Caller.SendAsync("GetAllMessages");
        await Clients.Caller.SendAsync("GetAllConversations");
        await base.OnConnectedAsync();
    }

    public async Task GetAllChatMessages(string chatId)
    {
        // Get all messages from the Cosmos DB
        var messages = await _cosmosDbRepository.ListItemsAsync("chatId", chatId);
        await Clients.Caller.SendAsync("GetMessages", messages);
    }
    // get specific conversation messages using conversationId
}