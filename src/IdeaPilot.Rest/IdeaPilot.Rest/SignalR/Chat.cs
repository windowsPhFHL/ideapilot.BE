using IdeaPilot.Rest.Data.Entities;

namespace IdeaPilot.Rest.SignalR;

public class Chat : BaseDocument
{
    //create id (initialixed to New Guid), workspaceid, timestamp (initialized to current timestamp), containerType (initialize it with `Chat`)
    public string id { get; set; } = Guid.NewGuid().ToString("N").Insert(0, "Chat_");
    public string WorkspaceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ContainerType { get; set; } = "Chat";
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; } = "Active";
}