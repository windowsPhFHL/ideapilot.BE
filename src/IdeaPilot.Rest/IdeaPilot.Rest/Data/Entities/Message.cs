namespace IdeaPilot.Rest.Data.Entities;

public sealed class Message
{
    public string AttributeName { get; set; }

    public Guid UserId { get; set; }

    public Guid ConversationId { get; set; }

    public string Text { get; set; }

    public string Status { get; set; }
}