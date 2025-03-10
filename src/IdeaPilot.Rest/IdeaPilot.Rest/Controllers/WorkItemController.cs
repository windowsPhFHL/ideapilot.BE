using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IdeaPilot.Rest.Data.Models;

[ApiController]
[Route("api/[controller]")]
public class WorkitemsController : ControllerBase
{
    private readonly AzureDevOpsService _azureDevOpsService;
    private readonly ILogger<WorkitemsController> _logger;

    public WorkitemsController(AzureDevOpsService azureDevOpsService, ILogger<WorkitemsController> logger)
    {
        _azureDevOpsService = azureDevOpsService;
        _logger = logger;
    }

    [HttpPost()]
    public async Task<IActionResult> CreateWorkItem([FromBody] WorkItemRequest request)
    {
        if (request.Deliverables == null || !request.Deliverables.Any())
        {
            _logger.LogWarning("CreateWorkItem called with no deliverables.");
            return BadRequest("At least one deliverable is required.");
        }

        _logger.LogInformation("Request contains {Count} deliverables", request.Deliverables.Count);

        var results = new List<object>();
        foreach (var deliverable in request.Deliverables)
        {
            _logger.LogInformation("Creating deliverable work item: {Title}", deliverable.Title);
            
            // Create parent deliverable
            var deliverableResponse = await _azureDevOpsService.CreateWorkItemAsync(deliverable.Title, deliverable.Description, "Deliverable");
            var deliverableResult = new { Deliverable = deliverableResponse, Tasks = new List<object>() };
            
            _logger.LogInformation("Deliverable work item created: {Id}", deliverableResponse.Id);
            // Create child tasks with parent reference
            if (deliverable.Tasks != null)
            {
                
                _logger.LogInformation("Deliverable contains {Count} tasks", deliverable.Tasks.Count);

                foreach (var task in deliverable.Tasks)
                {
                    _logger.LogInformation("Creating task work item: {Title} with parent {ParentId}", task.Title, deliverableResponse.Id);
                    var taskResponse = await _azureDevOpsService.CreateWorkItemAsync(
                        title: task.Title, 
                        description: task.Description,
                        "Task",
                        parentId: deliverableResponse.Id
                        );
                    deliverableResult.Tasks.Add(taskResponse);
                }
            }
            
            results.Add(deliverableResult);
        }

        return Ok(results);
    }
}
