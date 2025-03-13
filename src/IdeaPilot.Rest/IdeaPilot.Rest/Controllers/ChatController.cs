using Microsoft.AspNetCore.Mvc;
using IdeaPilot.Rest.Services;
using IdeaPilot.Rest.Data.Models;
using IdeaPilot.Rest.Data.Entities;
using IdeaPilot.Rest.SignalR;

namespace IdeaPilot.Rest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        //create crud operations for Chat
        //initialize chatRepository, _kernel and _logger
        private readonly ICosmosDbRepository<Chat> _chatRepository;
        private readonly ILogger<ChatController> _logger;
        private readonly ChatService _chatService;
        public ChatController(ICosmosDbRepository<Chat> chatRepository, ILogger<ChatController> logger, ChatService chatService)
        {
            _chatRepository = chatRepository;
            _logger = logger;
            _chatService = chatService;
        }

        [HttpGet("history/{sessionId}")]
        public async Task<IActionResult> GetChatHistory(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session ID is required.");
            }

            var history = await _chatService.GetChatHistoryAsync(sessionId);
            return Ok(history);
        }

        // DELETE api/<ChatController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(string id)
        {
            var chat = await _chatRepository.GetItemByPartitionKeyAsync(id);
            if (chat == null)
            {
                return NotFound();
            }
            await _chatRepository.DeleteItemAsync(id, id);
            return NoContent();
        }
    }
}
