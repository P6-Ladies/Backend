// src\Program.cs

using backend.Data;
using backend.Endpoints;
using backend.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

//Cross thing, something we might delete when going to production
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000") // Allow frontend
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Configure Services
builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment).ConfigureIdentity().ConfigureJwt(builder.Configuration).ConfigureAuthorizationPolicies().ConfigureSwagger();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// Map Endpoints
app.MapLoginEndpoint();
app.MapDeepSeekEndpoints();
app.MapUsersEndpoints();
app.MapConversationEndpoints();
app.MapMessageEndpoints();

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