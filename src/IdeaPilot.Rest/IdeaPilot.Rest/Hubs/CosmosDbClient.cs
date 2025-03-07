using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace IdeaPilot.Rest.Hubs;

public class CosmosDbClient
{
    CosmosClient cosmosClient;
    public CosmosDbClient()
    {
        //initialize the Cosmos DB client using managed identity
        this.cosmosClient = new CosmosClient("https://idea.documents.azure.com:443/",
            new DefaultAzureCredential());
    }

    public async Task<string> GetDataAsync()
    {

        // New instance of Database class referencing the server-side database
        Database database = await cosmosClient.CreateDatabaseAsync(
            id: "idea-pilot"
        );

        // New instance of Container class referencing the server-side container
        Container container = await database.CreateContainerAsync(
            id: "conversations",
            partitionKeyPath: "/chatId",
            throughput: 400
        );

        // Get the database and container
        //Database database = cosmosClient.GetDatabase("idea");
        //Container container = database.GetContainer("conversations");
        // Create a SQL query to retrieve all items in the container
        string sqlQuery = "SELECT * FROM conversations";
        QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
        FeedIterator<dynamic> queryResultSetIterator = container.GetItemQueryIterator<dynamic>(queryDefinition);
        // Iterate through the results and print them to the console
        string result = "";
        while (queryResultSetIterator.HasMoreResults)
        {
            FeedResponse<dynamic> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            foreach (var item in currentResultSet)
            {
                result += item.ToString() + "\n";
            }
        }
        return result;
    }

    internal async Task SaveMessageAsync(string user, string message, string wordspaceId)
    {
        //create a database if it doesn't exist
        Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("idea-pilot123");
        //log the database name
        Console.WriteLine($"Database name: {database.Id}");
        // Create a new item to save
        //Chat item = new Chat(
        //    user = user,
        //    message = message,
        //    wordspaceId = wordspaceId
        //);
        //// Get the database and container
        //Database database = cosmosClient.GetDatabase("idea-pilot");
        //Container container = database.GetContainer("conversations");
        //// Create the item in the container
        //try
        //{
        //    Chat response = await container.CreateItemAsync<Chat>(item, new PartitionKey(wordspaceId));
        //    // Print the response
        //    Console.WriteLine($"Item created with id: {response.id}");
        //}
        //catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        //{
        //    Console.WriteLine($"Item with id {item.id} already exists.");
        //}
        //throw new NotImplementedException();
    }
}