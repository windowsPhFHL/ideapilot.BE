using IdeaPilot.Rest.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using IdeaPilot.Rest.SignalR;

namespace IdeaPilot.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkspacesController : ControllerBase
{
    private readonly ICosmosDbRepository<Workspace> _workspaceRepository;
    private readonly ICosmosDbRepository<Chat> _chatRepository;
    private readonly ICosmosDbRepository<Message> _messageRepository;
    private readonly Kernel _kernel;
    private readonly ILogger<WorkspacesController> _logger;

    public WorkspacesController(
        ICosmosDbRepository<Workspace> workspaceRepository,
        ICosmosDbRepository<Chat> chatRepository,
        ICosmosDbRepository<Message> messageRepository,
        Kernel kernel,
        ILogger<WorkspacesController> logger
        )
    {
        _workspaceRepository = workspaceRepository;
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _kernel = kernel;
        _logger = logger;
    }

    // GET: api/<WorkspacesController>
    [HttpGet]
    public IEnumerable<Workspace> Get()
    {
        //list all workspaces in the cosmos db
        var workspaces = _workspaceRepository.ListItemsAsync("ContainerType", "Workspace").Result;

        //return the list of workspaces
        return workspaces;// workspaces.Select(workspace => new Workspace(workspace.WorkspaceId, workspace.Name, workspace.Description));
    }

    // POST api/<WorkspacesController>
    [HttpPost]
    public async Task<IActionResult> CreateWorkspace([FromBody] Workspace workspace)
    {
        if (workspace == null)
        {
            return BadRequest("Invalid workspace data");
        }

        //create a new workspace from the request body
        var newWorkspace = new Workspace
        {
            Name = workspace.Name,
            Description = workspace.Description,
            AttributeName = workspace.AttributeName
        };

        var createdWorkspace = await _workspaceRepository.CreateItemAsync(newWorkspace, newWorkspace.id);
        if (createdWorkspace == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating workspace");
        }
        return CreatedAtAction(nameof(GetWorkspace), new { id = createdWorkspace.id }, createdWorkspace);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkspace(string id)
    {
        var workspace = await _workspaceRepository.GetItemByPartitionKeyAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }
        return Ok(workspace);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] Workspace workspace)
    {
        if (workspace == null)
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

    [HttpGet("{id}/chats")]
    public async Task<IEnumerable<Chat>> GetWorkspaceChats(string id)
    {
        var chats = await _chatRepository.ListItemsAsync("WorkspaceId", id);
        return chats.OrderBy(c => c.CreatedOn);
    }

    [HttpGet("{id}/messages")]
    public async Task<IEnumerable<Message>> GetWorkspaceMessages(string id)
    {
        var messages = await _messageRepository.ListItemsAsync("WorkspaceId", id);
        return messages.OrderBy(m => m.CreatedOn);
    }

}