using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class WorkItemController : ControllerBase
{
    private readonly AzureDevOpsService _azureDevOpsService;
    private readonly ILogger<WorkItemController> _logger;

    public WorkItemController(AzureDevOpsService azureDevOpsService, ILogger<WorkItemController> logger)
    {
        _azureDevOpsService = azureDevOpsService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateWorkItem([FromBody] WorkItemRequest request)
    {
        if (string.IsNullOrEmpty(request.Title))
        {
            _logger.LogWarning("CreateWorkItem called with an empty title.");
            return BadRequest("Title is required.");
        }

        _logger.LogInformation("Creating work item with title: {Title}", request.Title);
        var response = await _azureDevOpsService.CreateWorkItemAsync(request.Title, request.Description);
        _logger.LogInformation("Work item created successfully with response: {Response}", response);

        return Ok(response);
    }
}

public class WorkItemRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
}
