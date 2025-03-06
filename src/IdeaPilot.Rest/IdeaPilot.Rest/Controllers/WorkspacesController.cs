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
        //var func = _kernel.CreateFunctionFromPrompt(string.Empty);

        //_kernel.InvokeAsync(func);

        return new string[] { "value1", "value2" };
    }

    //generate crud operations for the workspace
    [HttpPost]
    public async Task<IActionResult> CreateWorkspace([FromBody] Workspace workspace)
    {
        if (workspace == null)
        {
            return BadRequest("Workspace cannot be null");
        }
        var createdWorkspace = await _workspaceRepository.CreateItemAsync(workspace, workspace.WorkspaceId.ToString());
        return CreatedAtAction(nameof(Get), new { id = createdWorkspace.WorkspaceId }, createdWorkspace);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkspace(Guid id)
    {
        var workspace = await _workspaceRepository.GetItemAsync(id.ToString(), id.ToString());
        if (workspace == null)
        {
            return NotFound();
        }
        return Ok(workspace);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] Workspace workspace)
    {
        if (workspace == null || id != workspace.WorkspaceId)
        {
            return BadRequest("Invalid workspace data");
        }
        var updatedWorkspace = await _workspaceRepository.UpdateItemAsync(id.ToString(), id.ToString(), workspace);
        return Ok(updatedWorkspace);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkspace(Guid id)
    {
        var workspace = await _workspaceRepository.GetItemAsync(id.ToString(), id.ToString());
        if (workspace == null)
        {
            return NotFound();
        }
        await _workspaceRepository.DeleteItemAsync(id.ToString(), id.ToString());
        return NoContent();
    }


}