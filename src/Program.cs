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

// Map Endpoints
app.MapLoginEndpoint();
app.MapUsersEndpoints();

await app.ApplyMigrationsAsync();
await app.SeedDataAsync();

if (app.Environment.IsDevelopment())
{
    app.Run("http://0.0.0.0:5171");
}
else
{
    app.Run();
}
