using IdeaPilot.Rest.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace IdeaPilot.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkspacesController : ControllerBase
{
    private readonly ICosmosDbRepository<Workspace> _workspaceRepository;
    private readonly Kernel _kernel;
    private readonly ILogger<WorkspacesController> _logger;

    public WorkspacesController(ICosmosDbRepository<Workspace> workspaceRepository,Kernel kernel, ILogger<WorkspacesController> logger)
    {
        _workspaceRepository = workspaceRepository;
        _kernel = kernel;
        _logger = logger;
    }

    // GET: api/<WorkspacesController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        var func = _kernel.CreateFunctionFromPrompt(string.Empty);

        _kernel.InvokeAsync(func);

        return new string[] { "value1", "value2" };
    }

}