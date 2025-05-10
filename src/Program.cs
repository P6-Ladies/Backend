// src\Program.cs

using backend.Extensions;

public partial class Program
{
    public static void Main(string[] args)
        {

        var builder = WebApplication.CreateBuilder(args);

        // Configure services using the extension methods
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

        // Configure middleware using extension methods
        app.UseCors("AllowFrontend"); // Apply the CORS middleware here

        // Configure middleware
        app.ConfigureMiddleware();

        // Map the endpoints
        app.MapEndpoints();

        app.ApplyMigrationsAsync();
        app.SeedDataAsync();

        if (app.Environment.IsDevelopment())
        {
            app.Run("http://0.0.0.0:5171"); // Run on a specific port for development
        }
        else
        {
            app.Run(); // Use default settings in production
        }
    }
}