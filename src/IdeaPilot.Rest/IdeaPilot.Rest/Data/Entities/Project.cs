namespace IdeaPilot.Rest.Data.Entities;

public class Project :BaseDocument
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
}