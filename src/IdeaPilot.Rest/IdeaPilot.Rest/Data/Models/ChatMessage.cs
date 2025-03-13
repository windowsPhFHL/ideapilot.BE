using Newtonsoft.Json;

namespace IdeaPilot.Rest.Data.Models
{
    public class ChatMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("chatSessionId")]
        public string ChatSessionId { get; set; }  // 🔥 Added

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        public ChatMessage() { }

        public ChatMessage(string chatSessionId, string user, string message)
        {
            Id = Guid.NewGuid().ToString();
            ChatSessionId = chatSessionId;
            User = user;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }
    }
}
