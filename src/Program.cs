// src\Program.cs

using backend.Endpoints;
using backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure services using the extension methods
ConfigureServices(builder);

var app = builder.Build();

// Configure middleware
ConfigureMiddleware(app);

// Map the endpoints
MapEndpoints(app);

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

// Helper Methods

void ConfigureServices(WebApplicationBuilder builder)
{
    // Calls the extension method to configure services like database, identity, JWT, etc.
    builder.Services.ConfigureServices(builder.Configuration, builder.Environment);
}

void ConfigureMiddleware(WebApplication app)
{
    // Adds middleware like CORS, Authentication, Authorization, Swagger, etc.
    app.UseCors("AllowFrontend");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStaticFiles();
}

void MapEndpoints(WebApplication app)
{
    // Maps the API endpoints to the app
    app.MapLoginEndpoint();
    app.MapDeepSeekEndpoints();
    app.MapUsersEndpoints();
    app.MapConversationEndpoints();
}