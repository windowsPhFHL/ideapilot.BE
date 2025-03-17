namespace IdeaPilot.Rest.Data.Entities;

public sealed class Workspace : BaseDocument
{
       //add id as a property, initialize it with a new guid and append `Workspace_` as a prefix
    public string id { get; set; } = Guid.NewGuid().ToString("N").Insert(0, "Workspace_");
    public string Name { get; set; }
    public string Description { get; set; }

    //add Status initialized to "Active"
    public string Status { get; set; } = "Active";

    //add a property for the containerType and initialize it with `Workspace`
    public string ContainerType { get; set; } = "Workspace";

}