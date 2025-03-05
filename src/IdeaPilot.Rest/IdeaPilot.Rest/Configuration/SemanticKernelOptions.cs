namespace IdeaPilot.Rest.Configuration;

public class SemanticKernelOptions
{
    // For Azure OpenAI
    public string Endpoint { get; set; }         // e.g. https://<your-resource>.openai.azure.com/
    public string ApiKey { get; set; }           // Your Azure OpenAI key
    public string DeploymentName { get; set; }   // The name of the model deployment in Azure
    public string ApiVersion { get; set; }       // e.g. "2023-03-15-preview" or whichever version you use

    // For OpenAI (non-Azure) – optional
    public string OpenAIApiKey { get; set; }     // Your OpenAI API key
    public string OpenAIModelId { get; set; }    // e.g. "text-davinci-003"
}