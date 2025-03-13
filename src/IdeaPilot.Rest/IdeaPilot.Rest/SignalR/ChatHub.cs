using DocumentFormat.OpenXml.Wordprocessing;
using IdeaPilot.Rest.Data.Entities;
using IdeaPilot.Rest.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;

namespace IdeaPilot.Rest.SignalR;

public class ChatHub : Hub
{

    //create a construsctor that takes a Kernel and a CosmosDbClient
    private readonly ILogger<ChatHub> _logger;
    private readonly ICosmosDbRepository<Message> _cosmosDbRepository;

    //add kernel to the constructor
    public ChatHub(ILogger<ChatHub> logger, ICosmosDbRepository<Message> cosmosDbRepository)
    {
        _logger = logger;
        _cosmosDbRepository = cosmosDbRepository;
    }

    public async Task SendMessage(Message message)
    {
        // Log the message
        _logger.LogInformation($"Received message from {message.UserId}: {message.Text}");

        //create a new message from the message

        // Save the message to Cosmos DB
        await _cosmosDbRepository.CreateItemAsync(message, message.id);
        await Clients.All.SendAsync("ReceiveMessage", message.UserId, message);
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