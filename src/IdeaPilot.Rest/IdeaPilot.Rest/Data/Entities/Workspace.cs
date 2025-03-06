namespace IdeaPilot.Rest.Data.Entities;

public sealed class Workspace : BaseDocument 
{
    //add id as a property, initialize it with a new guid and append `Workspace_` as a prefix
    public string id { get; set; } = Guid.NewGuid().ToString("N").Insert(0, "Workspace_");
    public Guid WorkspaceId { get; set; }
    public string AttributeName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }

    //add a property for the containerType and initialize it with `Workspace`
    public string ContainerType { get; set; } = "Workspace";

}