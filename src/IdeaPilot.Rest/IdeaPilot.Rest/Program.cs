using Azure.Identity;
using IdeaPilot.Rest.Configuration;
using IdeaPilot.Rest.Data.Entities;
using IdeaPilot.Rest.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace IdeaPilot.Rest;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        var builder = WebApplication.CreateBuilder(args);

               builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        // Retrieve configuration values (e.g., from appsettings.json, secrets, environment variables, etc.)

        //string accountEndpoint = builder.Configuration["Cosmos:Endpoint"];
             builder.Services.Configure<CosmosDbOptions>(
            builder.Configuration.GetSection("CosmosDb")
        );

        // 1. Bind SemanticKernelOptions from config
        builder.Services.Configure<SemanticKernelOptions>(
            builder.Configuration.GetSection("SemanticKernel")
        );


        builder.Services.AddSingleton<Kernel>(serviceProvider =>
        {
            // (Optional) If you want to pull config from IOptions:
            var skOptions = serviceProvider.GetRequiredService<IOptions<SemanticKernelOptions>>().Value;

            var kernelBuilder = Kernel.CreateBuilder();

            // If using Azure OpenAI
            kernelBuilder.AddOpenAIChatCompletion(
                skOptions.DeploymentName, // "gpt-35-turbo" or similar
                skOptions.Endpoint, // e.g. "https://contoso.openai.azure.com/"
                skOptions.ApiKey // your Azure OpenAI Key
                // optional: apiVersion: "2023-03-15-preview"
            );
            return kernelBuilder.Build();
        });

        //register ChatHub class
        //builder.Services.AddSingleton(typeof(Hub<>), typeof(ChatHub));

        // 2. Register a singleton IKernel

        // 2. Create a Singleton CosmosClient (the recommended pattern)
        builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var cosmosDbOptions = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

            // You can configure CosmosClientOptions if needed
            var cosmosClientOptions = new CosmosClientOptions
            {
            };

            return new CosmosClient(cosmosDbOptions.AccountEndpoint, new DefaultAzureCredential(), cosmosClientOptions);
        });
        // Register the repository as a singleton or scoped, depending on your needs.
        // Usually, a Cosmos DB client can be a singleton.

        builder.Services.AddSingleton(typeof(ICosmosDbRepository<>), typeof(CosmosDbRepository<>));

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        builder.Services.AddSignalR();

        builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevOps"));
        builder.Services.AddSingleton<AzureDevOpsService>();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseRouting();

        // Enable the CORS policy
        app.UseCors("AllowAll");

        app.MapControllers();
        app.UseEndpoints(endpoints =>
        {
            // Map your controllers
            endpoints.MapControllers();

            // Map your SignalR hub
           // endpoints.MapHub<ChatHub>("/chatHub");
            endpoints.MapHub<ChatHub>("/chatHub").RequireCors("AllowAll");
        });

        app.Run();
    }
}