namespace IdeaPilot.Rest.Data.Entities;

public sealed class WorkspaceContributor : BaseDocument
{
    public string WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; }
}