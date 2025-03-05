namespace IdeaPilot.Rest.Data.Entities;

public interface ICosmosDbRepository<T> where T : class
{
    Task<T> CreateItemAsync(T item, string partitionKey);
    Task<T> GetItemAsync(string id, string partitionKey);
    Task<IEnumerable<T>> ListItemsAsync();
    Task<T> UpdateItemAsync(string id, string partitionKey, T item);
    Task DeleteItemAsync(string id, string partitionKey);
}