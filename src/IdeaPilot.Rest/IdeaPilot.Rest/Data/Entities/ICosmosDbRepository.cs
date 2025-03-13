namespace IdeaPilot.Rest.Data.Entities;

public interface ICosmosDbRepository<T> where T : class
{
    Task<T> CreateItemAsync(T item, string partitionKey);
    Task<T> GetItemAsync(string id, string partitionKey);

    //get item by partition key
    Task<T> GetItemByPartitionKeyAsync(string partitionKey);

    Task<IEnumerable<T>> ListItemsAsync();
    Task<T> UpdateItemAsync(string id, string partitionKey, T item);
    Task DeleteItemAsync(string id, string partitionKey);

    //create a method to create cosmos db container if not exists
    Task CreateContainerIfNotExistsAsync(string containerName, string partitionKeyPath);

    //create a method to create cosmos db database if not exists
    Task CreateDatabaseIfNotExistsAsync(string databaseName);

    //list items in the container based on a property
    Task<IEnumerable<T>> ListItemsAsync(string propertyName, string propertyValue);

    //create a method to get item in the container based on two properties
    Task<T> GetItemAsync(string propertyName1, string propertyValue1, string propertyName2, string propertyValue2);

    Task<T> GetItemSync(string propertyName1, string propertyValue1);

    //create a method to get an item in the container based on a key value pair of properties
    Task<T> GetItemAsync(Dictionary<string, string> properties);

    //create a method to get a list of items in the container based on a key value pair of properties
    Task<IEnumerable<T>> ListItemsAsync(Dictionary<string, string> properties);
}