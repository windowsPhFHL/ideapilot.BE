namespace IdeaPilot.Rest.Data.Entities
{
    public sealed class WorkspaceParticipant: BaseDocument
    {
        //add properties: id, workspaceId, userId, role, status
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string WorkspaceId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        //add containerType
        public string ContainerType { get; set; } = "WorkspaceParticipant";

    }
}
