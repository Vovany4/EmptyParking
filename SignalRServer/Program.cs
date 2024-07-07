using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SignalRServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllHeaders",
            builder =>
            {
                builder.WithOrigins("https://localhost:7230")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
});

var app = builder.Build();

app.UseRouting();


app.UseCors("AllowAllHeaders");

app.MapHub<ChatHub>("/chatHub");

app.Run();
