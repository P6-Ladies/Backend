// src\Endpoints\LoginEndpoint.cs
using backend.Security.Configuration;
using backend.Entities.Messages;
using backend.Entities.Users.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Diagnostics; // For Process and ProcessStartInfo
using Microsoft.AspNetCore.Mvc; // For [FromBody]
using System.Diagnostics;

namespace backend.Endpoints;

public static class DeepSeekEndpoints 
{

    private const string modelDirectory = "./Models/smol_lm_1.7b";
    private const string downloadScriptPath = "./src/Modules/download_model.py";
    private const string runScriptPath = "./src/Modules/run_model.py";

    public static RouteGroupBuilder MapDeepSeekEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("DeepSeek");
        group.MapPost("/generate", async ([FromBody] DeepSeekPromptDTO request) =>
        {
            if (!Directory.Exists(modelDirectory))
            {
                return Results.BadRequest("Model not downloaded. Please call /download-model first.");
            }

            var result = await ExecutePythonScript(
                scriptPath: runScriptPath,
                arguments: $"\"{modelDirectory}\" \"{request.Prompt}\""
            );

            return result;
        })
        .WithName("GenerateText")
        .WithTags("DeepSeek")
        .WithDescription("Generates text using the locally downloaded model.")
        .Accepts<DeepSeekPromptDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/DownloadModel", async () => {
            if (Directory.Exists(modelDirectory)) {
                return Results.Ok(new 
                {
                    Message = "Model already downloaded"
                });
            }
            var results = await ExecutePythonScript(scriptPath: downloadScriptPath, arguments: $"\"{modelDirectory}\"");
            return results;
        }).WithName("download model")
        .WithTags("DeepSeek")
        .Accepts<DeepSeekPromptDTO>("application/json");;

        return group;
    }

    //It execute a python script.
    private static async Task<IResult> ExecutePythonScript(string scriptPath, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"{scriptPath} {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };

        try
        {
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
            {
                return Results.BadRequest($"Python script error: {error}");
            }

            return Results.Ok(new { result = output });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Exception running Python script: {ex.Message}");
        }
    }
}