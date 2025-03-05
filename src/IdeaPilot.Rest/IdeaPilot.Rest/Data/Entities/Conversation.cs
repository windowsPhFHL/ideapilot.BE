namespace IdeaPilot.Rest.Data.Entities;

public sealed class Conversation : BaseDocument
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string Status { get; set; }
}