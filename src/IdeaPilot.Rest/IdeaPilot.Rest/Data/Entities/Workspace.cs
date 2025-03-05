namespace IdeaPilot.Rest.Data.Entities;

public sealed class Workspace : BaseDocument 
{
    public Guid WorkspaceId { get; set; }
    public string AttributeName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }

}