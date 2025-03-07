using IdeaPilot.Rest.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IdeaPilot.Rest.Controllers
{
    [Route("api/Workspace")]
    [ApiController]
    public class WorkspaceParticipantController : Controller
    {
        //inject the workspaceparticipants repository and logger
        private readonly ICosmosDbRepository<WorkspaceParticipant> _workspaceParticipantRepository;
        private readonly ICosmosDbRepository<Workspace> _workspaceRepository;
        private readonly ILogger<WorkspaceParticipantController> _logger;
        public WorkspaceParticipantController(ICosmosDbRepository<WorkspaceParticipant> workspaceParticipantRepository, ILogger<WorkspaceParticipantController> logger, ICosmosDbRepository<Workspace> workspaceRepository)
        {
            _workspaceParticipantRepository = workspaceParticipantRepository;
            _logger = logger;
            _workspaceRepository = workspaceRepository;
        }

        //create crud operations for the workspaceparticipants
        [HttpGet]
        public async Task<IActionResult> GetWorkspaceParticipants(string workspaceId)
        {

            //get all workspace participants from the cosmos db
            var workspaceParticipants = await _workspaceParticipantRepository.ListItemsAsync("WorkspaceId", workspaceId);
            if (workspaceParticipants == null)
            {
                return NotFound("No workspace participants found");
            }
            return Ok(workspaceParticipants);
        }

        //create endpoint for a user to join a workspace
        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinWorkspace(string id, [FromBody] string userId)
        {
            var workspace = await _workspaceRepository.GetItemSync("WorkspaceId",id);
            if (workspace == null)
            {
                return NotFound("Workspace not found");
            }

            //get FirstOrDefault workspace


            //add the user to the workspace
            var newWorkspaceParticipant = new WorkspaceParticipant
            {
                UserId = userId,
                WorkspaceId = workspace.WorkspaceId.ToString(),
            };
            var createdWorkspaceParticipant = await _workspaceParticipantRepository.CreateItemAsync(newWorkspaceParticipant, newWorkspaceParticipant.id);
            if (createdWorkspaceParticipant == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error joining workspace");
            }
            return CreatedAtAction(nameof(GetWorkspaceParticipants), new { id = createdWorkspaceParticipant.id }, createdWorkspaceParticipant);
        }

        //create endpoint for a user to leave a workspace
        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveWorkspace(string id, [FromBody] string userId)
        {

            var workspace = await _workspaceParticipantRepository.GetItemAsync("WorkspaceId", id.ToString(), "UserId", userId.ToString());
            if (workspace == null)
            {
                return NotFound();
            }
            await _workspaceParticipantRepository.DeleteItemAsync(workspace.id, id.ToString());
            return NoContent();
        }
    }
}
