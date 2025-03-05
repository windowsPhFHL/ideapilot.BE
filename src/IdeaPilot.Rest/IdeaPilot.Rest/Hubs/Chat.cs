
namespace IdeaPilotV1.Hubs
{
    public class Chat
    {
      

        public Chat(string user, string message, string wordspaceId)
        {
            Id = Guid.NewGuid().ToString();
            User = user;
            Message = message;
            WordspaceId = wordspaceId;
            Timestamp = DateTime.UtcNow;
        }

        //create id, user, message, wordspaceId, timestamp
        public string Id { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
        public string WordspaceId { get; set; }
        public DateTime Timestamp { get; set; }
        
    }
}