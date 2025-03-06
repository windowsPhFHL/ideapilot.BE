using IdeaPilot.Rest.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace IdeaPilot.Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        //initialize messagesRepository, _kernel and _logger
        private readonly ICosmosDbRepository<Message> _messagesRepository;
        private readonly Kernel _kernel;
        private readonly ILogger<MessagesController> _logger;
        public MessagesController(ICosmosDbRepository<Message> messagesRepository, Kernel kernel, ILogger<MessagesController> logger)
        {
            _messagesRepository = messagesRepository;
            _kernel = kernel;
            _logger = logger;
        }

        // GET: api/<MessagesController>
        [HttpGet]
        public IActionResult Get()
        {

            
            //var func = _kernel.CreateFunctionFromPrompt(string.Empty);
            //_kernel.InvokeAsync(func);
            return Ok(new string[] { "value1", "value2" });
        }
    }
}
