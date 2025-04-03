// src\Program.cs
using backend.Endpoints;
using backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Services
builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment)
    .ConfigureIdentity()
    .ConfigureJwt(builder.Configuration)
    .ConfigureAuthorizationPolicies()
    .ConfigureSwagger()
    .ConfigureEmailServices(builder.Configuration, builder.Environment)
    .ConfigureUserIdentity();

var app = builder.Build();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

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