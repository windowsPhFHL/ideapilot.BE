namespace IdeaPilot.Rest.Data.Models;

public class WorkItemRequest
{
    public required List<Deliverable> Deliverables { get; set; }
}

public class Deliverable
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public List<Task>? Tasks { get; set; }
}

public class Task
{
    public required string Title { get; set; }
    public required string Description { get; set; }
} 