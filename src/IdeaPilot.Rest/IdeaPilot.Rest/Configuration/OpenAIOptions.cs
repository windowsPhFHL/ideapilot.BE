namespace IdeaPilot.Rest.Configuration
{
    public class OpenAIOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string DeploymentName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string ApiVersionDate { get; set; } = string.Empty;

        public float Temperature { get; set; } = 1.01f;
        public int MaxTokens { get; set; } = 4096;
    }
}
