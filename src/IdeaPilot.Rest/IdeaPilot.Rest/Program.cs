using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Storage.Blobs;
using IdeaPilot.Rest.Configuration;
using IdeaPilot.Rest.Data.Entities;
using IdeaPilot.Rest.Services; // ✅ Import ChatService
using IdeaPilot.Rest.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdeaPilot.Rest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add controllers
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure CosmosDB Options
            builder.Services.Configure<CosmosDbOptions>(
                builder.Configuration.GetSection("CosmosDb")
            );

            // Configure OpenAI Options
            builder.Services.Configure<OpenAIOptions>(
                builder.Configuration.GetSection("OpenAI")
            );

            // Configure Azure Blob Storage Options
            builder.Services.Configure<AzureBlobStorageOptions>(
                builder.Configuration.GetSection("AzureBlobStorage")
            );

            // Register OpenAIClient as Singleton
            builder.Services.AddSingleton<OpenAIClient>(serviceProvider =>
            {
                var openAiOptions = serviceProvider.GetRequiredService<IOptions<OpenAIOptions>>().Value;

                if (!string.IsNullOrEmpty(openAiOptions.ApiKey))
                {
                    return new OpenAIClient(
                        new Uri(openAiOptions.Endpoint),
                        new AzureKeyCredential(openAiOptions.ApiKey)
                    );
                }
                else
                {
                    return new OpenAIClient(
                        new Uri(openAiOptions.Endpoint),
                        new DefaultAzureCredential()
                    );
                }
            });

            // Register CosmosClient as Singleton
            builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
            {
                var cosmosDbOptions = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
                var cosmosClientOptions = new CosmosClientOptions();

                return new CosmosClient(cosmosDbOptions.AccountEndpoint, new DefaultAzureCredential(), cosmosClientOptions);
            });

            // Register BlobServiceClient as Singleton
            builder.Services.AddSingleton<BlobServiceClient>(serviceProvider =>
            {
                var blobStorageOptions = serviceProvider.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value;
                return new BlobServiceClient(new Uri(blobStorageOptions.Endpoint), new DefaultAzureCredential());
            });

            // Register CosmosDB Repository
            builder.Services.AddSingleton(typeof(ICosmosDbRepository<>), typeof(CosmosDbRepository<>));

            // Register ChatService (Fixes "Unable to resolve service" error)
            builder.Services.AddSingleton<ChatService>();

            // CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Register SignalR
            builder.Services.AddSignalR();

            // Register Azure DevOps Settings and Service
            builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevOps"));
            builder.Services.AddSingleton<AzureDevOpsService>();

            // Logging Configuration
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var app = builder.Build();

            // Enable Swagger for API Documentation
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseRouting();
            app.UseCors("AllowAll");

            // Map Controllers and SignalR Hub
            app.MapControllers();
            app.MapHub<ChatHub>("/chatHub").RequireCors("AllowAll");

            app.Run();
        }
    }
}
