namespace IdeaPilot.Rest.Data.Entities;

public class BaseDocument
{
    public DateTime UpdatedOn { get; set; } = DateTime.Now;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}