namespace IdeaPilot.Rest.Configuration;

public class CosmosDbOptions
{
    public string AccountEndpoint { get; set; }
    public string AuthKey { get; set; }
    public string DatabaseId { get; set; }

    // For a single-container approach:
    public string ContainerId { get; set; }
    public string PartitionKeyPath { get; set; } = "/partitionKey";

    // You can add RU/s or any additional settings as needed
    public int Throughput { get; set; } = 400;
}