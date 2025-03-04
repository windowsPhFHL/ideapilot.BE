using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace IdeaPilot.Rest;

public class ChatHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    // Called by clients to send a message to everyone.
    public async Task SendMessage(string user, string message)
    {
        // This will call the client-side method 'ReceiveMessage' on all connected clients.
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

               builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
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