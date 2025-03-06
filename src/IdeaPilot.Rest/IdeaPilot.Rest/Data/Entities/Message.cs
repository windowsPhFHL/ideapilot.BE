namespace IdeaPilot.Rest.Data.Entities;

public sealed class Message
{

    //add Id property
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string AttributeName { get; set; }

    public Guid UserId { get; set; }

    public Guid ConversationId { get; set; }

    public string Text { get; set; }

    public string Status { get; set; }
}