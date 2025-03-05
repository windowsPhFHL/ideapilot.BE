namespace IdeaPilot.Rest.Data.Entities;

public class BaseDocument
{
    public DateTime UpdatedOn { get; set; }
    public DateTime Created { get; set; }
    public string PartitionKey { get; set; }
    public string Id { get; set; } = Guid.NewGuid().ToString();
}