using Newtonsoft.Json;

namespace IdeaPilot.Rest.Data.Models
{
    public class ChatSession
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sessionName")]
        public string SessionName { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        public ChatSession()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
