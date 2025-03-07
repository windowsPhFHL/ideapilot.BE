namespace IdeaPilot.Rest.Data.Entities;

public sealed class Message
{
    //add id as a property, initialize it with a new guid and append `Message_` as a prefix
    public string id { get; set; } = Guid.NewGuid().ToString("N").Insert(0, "Message_");

    public Guid UserId { get; set; }

    public Guid ChatId { get; set; }

    public string Text { get; set; }

    public string Status { get; set; }

    //add a property for the containerType and initialize it with `Message`
    public string ContainerType { get; set; } = "Message";
}