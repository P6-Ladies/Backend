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
    private const string downloadScriptPath = "./Modules/download_model.py";
    private const string runScriptPath = "./Modules/run_model.py";

    public static RouteGroupBuilder MapDeepSeekEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("DeepSeek");
        group.MapPost("/generate", async ([FromBody] DeepSeekPromptDTO request) =>
        {
            if (!Directory.Exists(modelDirectory))
            {
                return Results.BadRequest(new {
                    message = "Model not downloaded. Please call /download-model first.",
                    solution = "Please call other endpoint first",
                    expectedPath = Path.GetFullPath(modelDirectory)
                });
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
                    Message = "Model already downloaded",
                    path = Path.GetFullPath(modelDirectory) 
                });
            }
            var results = await ExecutePythonScript(
                scriptPath: downloadScriptPath, 
                arguments: $"\"{modelDirectory}\""
            );
            if(results is OkObjectResult okResult && okResult.Value != null) {
                return Results.Ok(new {
                    message = "Model Downloaded Succesfully",
                    path = Path.GetFullPath(modelDirectory),
                    details = okResult.Value
                });
            }
                // ✅ Add a fallback response:
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Failed to download model.",
                detail: "The Python script did not return a valid result."
            );
        })
        .WithName("download model")
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
            // Start process and set up async reading
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender,e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) errorBuilder.AppendLine(e.Data);
            };

            process.Start();

            // Begin async Reading
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var timeout = TimeSpan.FromSeconds(3600);
            bool exited = process.WaitForExit((int)timeout.TotalMilliseconds);  

            if (!exited)
            {
                process.Kill(true);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: $"Python script timed out after {timeout.TotalSeconds} seconds"
                );
            }

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            // Check Exit Code
            if (process.ExitCode != 0)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError, 
                    detail: $"Python Script Failed with exit code: {process.ExitCode}\n" + 
                    $"Error output: {error}\n" + 
                    $"Standard Ouptut: \n{output}"
                );
            }

            return Results.Ok(new {
                results = output,
                warnings = string.IsNullOrWhiteSpace(error) ? null : error
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: $"Exception running Python script: {ex.GetType().Name}\n" + 
                $"Message: {ex.Message} \n" +
                $"Stack trace: {ex.StackTrace}");
        }
    }
}