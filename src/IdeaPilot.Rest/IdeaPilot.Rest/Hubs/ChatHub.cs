using Microsoft.AspNetCore.SignalR;

namespace IdeaPilotV1.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Received message from {user}: {message}");

            //AiFoundry.RunAsync(user, message).Wait();


            CosmosDbClient cosmosDbClient = new CosmosDbClient();
            // Save the message to Cosmos DB
            await cosmosDbClient.SaveMessageAsync(user, message, Guid.NewGuid().ToString());


            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}