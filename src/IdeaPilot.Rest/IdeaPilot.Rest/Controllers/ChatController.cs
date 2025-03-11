//using IdeaPilot.Rest.Data.Entities;
//using IdeaPilot.Rest.SignalR;
//using Microsoft.AspNetCore.Mvc;
//using Azure.AI.OpenAI;

//namespace IdeaPilot.Rest.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ChatController : Controller
//    {
//        private readonly ICosmosDbRepository<Chat> _chatRepository;
//        private readonly ICosmosDbRepository<Message> _messageRepository;
//        private readonly OpenAIClient _openAIClient;
//        private readonly ILogger<ChatController> _logger;

//        public ChatController(
//            ICosmosDbRepository<Chat> chatRepository,
//            ICosmosDbRepository<Message> messageRepository,
//            OpenAIClient openAIClient,
//            ILogger<ChatController> logger)
//        {
//            _chatRepository = chatRepository;
//            _messageRepository = messageRepository;
//            _openAIClient = openAIClient;
//            _logger = logger;
//        }

//        // GET: api/<ChatController>
//        [HttpGet]
//        public IActionResult Get()
//        {
//            //list all chats in the cosmos db
//            var chats = _chatRepository.ListItemsAsync("containerType", "Chat").Result;
//            //return the list of chats
//            return Ok(chats);
//        }

//        // POST api/<ChatController>
//        [HttpPost]
//        public async Task<IActionResult> CreateChat([FromBody] Chat chat)
//        {
//            if (chat == null)
//            {
//                return BadRequest("Invalid chat data");
//            }
//            //create a new chat from the request body
//            var newChat = new Chat
//            {
//                Name = chat.Name,
//                Description = chat.Description
//            };

//            var createdChat = await _chatRepository.CreateItemAsync(newChat, newChat.id);

//            if (createdChat == null)
//            {
//                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating chat");
//            }
//            return CreatedAtAction(nameof(GetChat), new { id = createdChat.id }, createdChat);
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetChat(string id)
//        {
//            var chat = await _chatRepository.GetItemByPartitionKeyAsync(id);
//            if (chat == null)
//            {
//                return NotFound();
//            }
//            return Ok(chat);
//        }

//        // PUT api/<ChatController>/5
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateChat(string id, [FromBody] Chat chat)
//        {
//            if (chat == null)
//            {
//                return BadRequest("Invalid chat data");
//            }
//            var updatedChat = await _chatRepository.UpdateItemAsync(id, id, chat);
//            if (updatedChat == null)
//            {
//                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating chat");
//            }
//            return Ok(updatedChat);
//        }

//        // DELETE api/<ChatController>/5
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteChat(string id)
//        {
//            var chat = await _chatRepository.GetItemByPartitionKeyAsync(id);
//            if (chat == null)
//            {
//                return NotFound();
//            }
//            await _chatRepository.DeleteItemAsync(id, id);
//            return NoContent();
//        }

//        [HttpGet("{id}/messages")]
//        public async Task<IEnumerable<Message>> GetChatMessages(string id)
//        {
//            var messages = await _messageRepository.ListItemsAsync("ChatId", id);
//            return messages.OrderBy(m => m.CreatedOn);
//        }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using IdeaPilot.Rest.Services;
using IdeaPilot.Rest.Data.Models;

namespace IdeaPilot.Rest.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.ChatSessionId) || string.IsNullOrEmpty(message.Message))
            {
                return BadRequest("Invalid message data");
            }

            await _chatService.SaveChatMessageAsync(message.ChatSessionId, message.User, message.Message);

            string chatResponse = await _chatService.GetChatResponseAsync(message.Message);
            return Ok(new { result = chatResponse });
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

        [HttpDelete("history/{sessionId}")]
        public async Task<IActionResult> DeleteChatHistory(string sessionId)
        {
            return Ok($"✅ All messages under Session ID: {sessionId} have been deleted");
        }
    }
}
