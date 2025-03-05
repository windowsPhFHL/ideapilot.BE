using IdeaPilot.Rest.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IdeaPilot.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkspacesController : ControllerBase
{
    private readonly ICosmosDbRepository<Workspace> _workspaceRepository;
    private readonly ILogger<WorkspacesController> _logger;

    public WorkspacesController(ICosmosDbRepository<Workspace> workspaceRepository, ILogger<WorkspacesController> logger)
    {
        _workspaceRepository = workspaceRepository;
        _logger = logger;
    }

    // GET: api/<WorkspacesController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

}