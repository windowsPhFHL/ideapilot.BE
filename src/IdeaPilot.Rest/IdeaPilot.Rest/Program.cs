﻿using IdeaPilot.Rest.SignalR;
using Microsoft.AspNetCore.SignalR;

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