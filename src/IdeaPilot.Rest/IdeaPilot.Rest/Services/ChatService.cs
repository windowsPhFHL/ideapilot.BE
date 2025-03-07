using IdeaPilot.Rest.Data.Entities;
using IdeaPilot.Rest.SignalR;
using Microsoft.SemanticKernel;

namespace IdeaPilot.Rest.Services
{
    public class ChatService
    {

        //initialize the chat service with the kernel, cosmosdbclient and logger
        private readonly Kernel _kernel;
        private readonly ILogger<ChatService> _logger;
        private readonly ICosmosDbRepository<Chat> _cosmosDbRepository;
        public ChatService(Kernel kernel, ILogger<ChatService> logger, ICosmosDbRepository<Chat> cosmosDbRepository)
        {
            _kernel = kernel;
            _logger = logger;
            _cosmosDbRepository = cosmosDbRepository;
        }
           
        internal async Task PostMessage(Message newMessage)
        {
            //get chats from the cosmos db
            var chats = await _cosmosDbRepository.ListItemsAsync();
        }
    }
}
