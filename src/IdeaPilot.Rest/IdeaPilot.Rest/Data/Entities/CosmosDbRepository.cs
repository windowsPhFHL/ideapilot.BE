﻿using IdeaPilot.Rest.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Microsoft.Extensions.Options;

namespace IdeaPilot.Rest.Data.Entities;

public class CosmosDbRepository<T> : ICosmosDbRepository<T> where T : class
{
    private readonly Container _container;

    /// <summary>
    /// Initializes the Cosmos DB client, creates the database/container if they don't exist.
    /// </summary>
    /// <param name="accountEndpoint">Cosmos DB endpoint URI.</param>
    /// <param name="databaseId">Name of the database.</param>
    /// <param name="containerId">Name of the container.</param>
    /// <param name="partitionKeyPath">Partition key path (e.g., "/partitionKey").</param>
    public CosmosDbRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> cosmosDbOptions)
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            // Configure client options as needed, e.g.:
            // AllowBulkExecution = true
        };

        //initialize the container
        _container = cosmosClient.GetContainer(cosmosDbOptions.Value.DatabaseId, cosmosDbOptions.Value.ContainerId);
    }

    /// <summary>
    /// Create a new document in the container.
    /// </summary>
    /// <param name="item">The item to create.</param>
    /// <param name="partitionKey">Partition key value (e.g. the item’s partition key).</param>
    /// <returns>The created item.</returns>
    public async Task<T> CreateItemAsync(T item, string partitionKey)
    {
        var response = await _container.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(partitionKey));
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
            ItemResponse<T> response = await _container.ReadItemAsync<T>(id, new Microsoft.Azure.Cosmos.PartitionKey(partitionKey));
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
        var response = await _container.UpsertItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(partitionKey));
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
        await _container.DeleteItemAsync<T>(id, new Microsoft.Azure.Cosmos.PartitionKey(partitionKey));
    }

    public async Task CreateContainerIfNotExistsAsync(string containerName, string partitionKeyPath)
    {
        // Create the container if it does not exist
        var containerResponse = _container.Database.CreateContainerIfNotExistsAsync(containerName, partitionKeyPath, 400).Result;
        if (containerResponse.StatusCode == System.Net.HttpStatusCode.Created)
        {
            Console.WriteLine($"Container {containerName} created successfully.");
        }
        else
        {
            Console.WriteLine($"Container {containerName} already exists.");
        }
    }

    public async Task CreateDatabaseIfNotExistsAsync(string databaseName)
    {
        // Create the database if it does not exist
        var databaseResponse = _container.Database.Client.CreateDatabaseIfNotExistsAsync(databaseName).Result;
        if (databaseResponse.StatusCode == System.Net.HttpStatusCode.Created)
        {
            Console.WriteLine($"Database {databaseName} created successfully.");
        }
        else
        {
            Console.WriteLine($"Database {databaseName} already exists.");
        }
    }

    async Task<IEnumerable<T>> ICosmosDbRepository<T>.ListItemsAsync(string propertyName, string propertyValue)
    {
        // Create a SQL query to filter items based on the property name and value
        string sqlQuery = $"SELECT * FROM c WHERE c.{propertyName} = @propertyValue";
        var queryDefinition = new QueryDefinition(sqlQuery)
            .WithParameter("@propertyValue", propertyValue);
        var query = _container.GetItemQueryIterator<T>(queryDefinition);
        var results = new List<T>();
        while (query.HasMoreResults)
        {
            FeedResponse<T> response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    Task<T> ICosmosDbRepository<T>.GetItemByPartitionKeyAsync(string partitionKey)
    {
       // Get item by partition key
        var query = _container.GetItemQueryIterator<T>($"SELECT * FROM c WHERE c.id = '{partitionKey}'");

        var results = new List<T>();

        while (query.HasMoreResults)
        {
            FeedResponse<T> response = query.ReadNextAsync().Result;
            results.AddRange(response);
        }
        return Task.FromResult(results.FirstOrDefault());

    }
}