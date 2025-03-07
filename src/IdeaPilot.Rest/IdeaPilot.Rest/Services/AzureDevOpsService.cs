using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

public class AzureDevOpsService
{
    private readonly HttpClient _httpClient;
    private readonly string _organization;
    private readonly string _project;
    private readonly string _personalAccessToken;
    private readonly string _areaPath;
    private readonly ILogger<AzureDevOpsService> _logger;

    public AzureDevOpsService(IOptions<AzureDevOpsSettings> settings, ILogger<AzureDevOpsService> logger)
    {
        var config = settings.Value;
        _httpClient = new HttpClient();
        _organization = config.Organization ?? throw new ArgumentNullException(nameof(_organization));
        _project = config.Project ?? throw new ArgumentNullException(nameof(_project));
        _personalAccessToken = config.PersonalAccessToken ?? throw new ArgumentNullException(nameof(_personalAccessToken));
        _areaPath = config.AreaPath ?? throw new ArgumentNullException(nameof(_areaPath));
        _logger = logger;

        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        
        // Log a redacted version of the auth header for debugging
        var redactedPAT = _personalAccessToken.Length > 4 
            ? $"{_personalAccessToken.Substring(0, 4)}..." 
            : "***";
        _logger.LogDebug("Auth header set with PAT starting with: {RedactedPAT}", redactedPAT);

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json-patch+json"));

        _logger.LogInformation("Harro");
        _logger.LogInformation("AzureDevOpsService initialized with organization: {Organization}, project: {Project}", _organization, _project);
    }

    public async Task<WorkItemResponse> CreateWorkItemAsync(string title, string description, string type, int? parentId = null)
    {
        var relations = parentId.HasValue ? new[]
        {
            new
            {
                rel = "System.LinkTypes.Hierarchy-Reverse",
                url = $"{_organization}/{_project}/_apis/wit/workItems/{parentId}"
            }
        } : null;

        //var url = $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/workitems/$Task?api-version=7.1";
        var url = $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/workitems/${type}?api-version=7.1";

        _logger.LogInformation("Creating work item using URL: {Url} and area Path: {areaPath}", url, _areaPath);

        var workItemData = new List<object>
        {
            new { op = "add", path = "/fields/System.Title", value = title },
            new { op = "add", path = "/fields/System.Description", value = description },
            new { op = "add", path = "/fields/System.AreaPath", value = _areaPath }
        };

        if (parentId.HasValue)
        {
            //workItemData.Add(new { op = "add", path = "/relations", value = relations });
            workItemData.Add(new 
            {
                op = "add",
                path = "/relations/-",
                value = new
                {
                    rel = "System.LinkTypes.Hierarchy-Reverse",
                    url = $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/workItems/{parentId.Value}",
                    attributes = new { comment = "Linking task to deliverable" }
                }
            });            
        }

        var jsonContent = JsonConvert.SerializeObject(workItemData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json-patch+json");

        _logger.LogInformation("Creating work item with title: {Title}", title);

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            _logger.LogInformation("Got status code {statusCode}", response.StatusCode);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Permission denied. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new UnauthorizedAccessException("Permission denied. Your PAT token might not have the required permissions (needs vso.work_write scope).");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authentication failed. Please check your Personal Access Token.");
                throw new UnauthorizedAccessException("Authentication failed with Azure DevOps API.");
            }
            
            // Log the status code for debugging
            _logger.LogInformation("Response status code: {StatusCode}", response.StatusCode);

            response.EnsureSuccessStatusCode(); // This will throw if the response code is not 2xx

            _logger.LogInformation("Work item created successfully.");
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WorkItemResponse>(responseContent);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Request failed: {Message}", ex.Message);
            _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
            throw; // Re-throw the exception to propagate it if needed
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
            _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
            throw; // Re-throw the exception to propagate it if needed
        }
    }

}

public class WorkItemResponse
{
    public int Id { get; set; }
    // ... other properties you're returning ...
}
