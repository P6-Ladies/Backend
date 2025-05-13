// src\Program.cs

using Backend.Data;
using Backend.Endpoints;
using Backend.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

// Configure Services
builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment)
    .ConfigureIdentity()
    .ConfigureJwt(builder.Configuration)
    .ConfigureAuthorizationPolicies()
    .ConfigureSwagger()
    .ConfigureEmailServices(builder.Configuration, builder.Environment)
    .ConfigureUserIdentity()
    .ConfigureCors();

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
