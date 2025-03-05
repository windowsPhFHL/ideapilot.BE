using IdeaPilot.Rest.Data.Entities;
using IdeaPilot.Rest.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IdeaPilot.Rest.SignalR;

public class ChatHub : Hub
{

    //create a construsctor that takes a Kernel and a CosmosDbClient
    private readonly ILogger<ChatHub> _logger;
    private readonly ICosmosDbRepository<Chat> _cosmosDbClient;
    public ChatHub( ILogger<ChatHub> logger, ICosmosDbRepository<Chat> cosmosDbClient)
    {
        _logger = logger;
        _cosmosDbClient = cosmosDbClient;
    }

    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine($"Received message from {user}: {message}");

        //AiFoundry.RunAsync(user, message).Wait();


        //CosmosDbClient cosmosDbClient = new CosmosDbClient();
        // Save the message to Cosmos DB
        //await cosmosDbClient.SaveMessageAsync(user, message, Guid.NewGuid().ToString());


        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}