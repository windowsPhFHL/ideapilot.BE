using IdeaPilot.Rest.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using IdeaPilot.Rest.SignalR;

namespace IdeaPilot.Rest.Controllers;

[Route("api/workspaces")]
[ApiController]
public class WorkspacesController : ControllerBase
{
    private readonly ICosmosDbRepository<Workspace> _workspaceRepository;
    private readonly ICosmosDbRepository<Chat> _chatRepository;
    private readonly ICosmosDbRepository<Message> _messageRepository;
    private readonly ILogger<WorkspacesController> _logger;

    public WorkspacesController(
        ICosmosDbRepository<Workspace> workspaceRepository,
        ICosmosDbRepository<Chat> chatRepository,
        ICosmosDbRepository<Message> messageRepository,
        ILogger<WorkspacesController> logger
        )
    {
        _workspaceRepository = workspaceRepository;
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    // GET: api/<WorkspacesController>
    [HttpGet]
    public IEnumerable<Workspace> Get()
    {
        //list all workspaces in the cosmos db
        Dictionary<string, string> properties = new Dictionary<string, string>
        {
            { "ContainerType", "Workspace" }
        };
        var workspaces = _workspaceRepository.ListItemsAsync(properties).Result;

        //return the list of workspaces
        return workspaces;
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
            id = Guid.NewGuid().ToString("N").Insert(0, "Workspace_"),
            Name = workspace.Name,
            Description = workspace.Description,
            CreatedOn = DateTime.UtcNow,
            ContainerType = "Workspace"
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
    public async Task<IActionResult> UpdateWorkspace(string id, [FromBody] Workspace workspace)
    {
        if (workspace == null)
        {
            return BadRequest("Invalid workspace data");
        }

        //get the workspace from the cosmos db
        var existingWorkspace = await _workspaceRepository.GetItemAsync(id.ToString(), id.ToString());

        if (existingWorkspace == null)
        {
            return NotFound();
        }

        var updatedWorkspace = await _workspaceRepository.UpdateItemAsync(id.ToString(), id.ToString(), workspace);
        return Ok(updatedWorkspace);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkspace(string id)
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
        Dictionary<string, string> properties = new Dictionary<string, string>
        {
            { "WorkspaceId", id },
            { "ContainerType", "Chat" }
        };
        var chats = await _chatRepository.ListItemsAsync(properties);
        return chats.OrderBy(c => c.CreatedOn);
    }

    [HttpGet("{id}/messages")]
    public async Task<IEnumerable<Message>> GetWorkspaceMessages(string id)
    {
        Dictionary<string, string> properties = new Dictionary<string, string>
        {
            { "WorkspaceId", id },
            { "ContainerType", "Message" }
        };
        var messages = await _messageRepository.ListItemsAsync(properties);
        return messages.OrderBy(m => m.CreatedOn);
    }

}