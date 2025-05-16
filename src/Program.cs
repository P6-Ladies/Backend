// src\Program.cs

using backend.Data;
using backend.Endpoints;
using backend.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

// Configure Services
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

// Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy.WithOrigins("http://localhost:3000") // Adjust to match frontend URL
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

var app = builder.Build();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseCors("AllowFrontend");

// Map the endpoints
app.MapEndpoints();

await app.ApplyMigrationsAsync();
await app.SeedDataAsync();

if (app.Environment.IsDevelopment())
{
    app.Run("http://0.0.0.0:5171"); // Run on a specific port for development
}
else
{
    app.Run(); // Use default settings in production
}
