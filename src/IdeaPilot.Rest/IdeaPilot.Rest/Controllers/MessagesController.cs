using IdeaPilot.Rest.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.AI.OpenAI;
using IdeaPilot.Rest.Services;

namespace IdeaPilot.Rest.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ICosmosDbRepository<Message> _messagesRepository;
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<MessagesController> _logger;
        private readonly ChatService _chatService;

        public MessagesController(
            ICosmosDbRepository<Message> messagesRepository,
            OpenAIClient openAIClient,
            ChatService chatService,
            ILogger<MessagesController> logger)
        {
            _messagesRepository = messagesRepository;
            _openAIClient = openAIClient;
            _logger = logger;
            _chatService = chatService;
        }

        // GET: api/messages
        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {

            Dictionary<string, string> props = new Dictionary<string, string>();
            //set the partition key to the user id
            props.Add("ContainerType", "Message");
            //list all messages in the cosmos db
            var messages = await _messagesRepository.ListItemsAsync(props);
            //return the list of messages
            return Ok(messages);
        }  

        // endpoint to create a new message
        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] Message message)
        {
            if (message == null)
            {
                return BadRequest("Invalid message data");
            }

            //create a new message from the request body
            var newMessage = new Message
            {
                UserId = message.UserId,
                ChatId = message.ChatId,
                WorkspaceId = message.WorkspaceId,
                Text = message.Text,
                Status = message.Status,
            };

            var createdMessage = await _messagesRepository.CreateItemAsync(newMessage, newMessage.id);
            if (createdMessage == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating message");
            }


            await _chatService.ProcessModelMessages(newMessage);

            return CreatedAtAction(nameof(GetMessage), new { id = newMessage.id }, newMessage);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(string id)
        {
            var message = await _messagesRepository.GetItemByPartitionKeyAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(string id, [FromBody] Message message)
        {
            if (message == null)
            {
                return BadRequest("Invalid message data");
            }
            var updatedMessage = await _messagesRepository.UpdateItemAsync(id, id, message);
            return Ok(updatedMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            var message = await _messagesRepository.GetItemAsync(id, id);
            if (message == null)
            {
                return NotFound();
            }
            await _messagesRepository.DeleteItemAsync(id, id);
            return NoContent();
        }
    }
}
