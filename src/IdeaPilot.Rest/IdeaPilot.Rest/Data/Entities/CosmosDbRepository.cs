using Microsoft.Azure.Cosmos;

namespace IdeaPilot.Rest.Data.Entities;

public class CosmosDbRepository<T> where T : class
{
    private readonly Container _container;

    /// <summary>
    /// Initializes the Cosmos DB client, creates the database/container if they don't exist.
    /// </summary>
    /// <param name="accountEndpoint">Cosmos DB endpoint URI.</param>
    /// <param name="authKey">Cosmos DB primary key.</param>
    /// <param name="databaseId">Name of the database.</param>
    /// <param name="containerId">Name of the container.</param>
    /// <param name="partitionKeyPath">Partition key path (e.g., "/partitionKey").</param>
    public CosmosDbRepository(
        string accountEndpoint,
        string authKey,
        string databaseId,
        string containerId,
        string partitionKeyPath)
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            // Configure client options as needed, e.g.:
            // AllowBulkExecution = true
        };

        var client = new CosmosClient(accountEndpoint, authKey, cosmosClientOptions);

        // Create the database if it does not exist
        Database database = client.CreateDatabaseIfNotExistsAsync(databaseId).Result;

        // Create the container if it does not exist
        _container = database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath, 400).Result;
    }

    /// <summary>
    /// Create a new document in the container.
    /// </summary>
    /// <param name="item">The item to create.</param>
    /// <param name="partitionKey">Partition key value (e.g. the item’s partition key).</param>
    /// <returns>The created item.</returns>
    public async Task<T> CreateItemAsync(T item, string partitionKey)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
        return response.Resource;
    }

    /// <summary>
    /// Read a document by its ID and partition key.
    /// </summary>
    /// <param name="id">The ID of the item.</param>
    /// <param name="partitionKey">The partition key value of the item.</param>
    /// <returns>The item if found, otherwise null.</returns>
    public async Task<T> GetItemAsync(string id, string partitionKey)
    {
        try
        {
            ItemResponse<T> response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Lists all documents in the container. 
    /// (Consider adding filters or partition key constraints for production usage.)
    /// </summary>
    /// <returns>A list of all items.</returns>
    public async Task<IEnumerable<T>> ListItemsAsync()
    {
        var query = _container.GetItemQueryIterator<T>("SELECT * FROM c");
        var results = new List<T>();

        while (query.HasMoreResults)
        {
            FeedResponse<T> response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    /// <summary>
    /// Update an existing document.
    /// </summary>
    /// <param name="id">ID of the item.</param>
    /// <param name="partitionKey">Partition key value of the item.</param>
    /// <param name="item">The updated item.</param>
    /// <returns>The updated item.</returns>
    public async Task<T> UpdateItemAsync(string id, string partitionKey, T item)
    {
        var response = await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));
        return response.Resource;
    }

    /// <summary>
    /// Delete a document by ID.
    /// </summary>
    /// <param name="id">ID of the item.</param>
    /// <param name="partitionKey">Partition key value of the item.</param>
    /// <returns></returns>
    public async Task DeleteItemAsync(string id, string partitionKey)
    {
        await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
    }
}