namespace IdeaPilot.Rest.Data.Entities;

public sealed class Response : BaseDocument
{
    public Guid ConverstationId { get; set; }
    public Guid UserId { get; set; }
    public Guid MessageId { get; set; }
    public string Status { get; set; }
}