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


namespace backend.Endpoints;

public static class DeepSeekEndpoints 
{

    private static readonly string ModelsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "app", "Models"));
    private const string ModelName = "smol_lM_1.7b";
    private static string ModelDirectory => Path.Combine(ModelsRoot, ModelName);
    private const string DownloadScriptName = "download_model.py";
    private const string RunScriptName = "run_model.py";
    private static readonly string ScriptsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "app", "Modules"));
    //output is whatever we accept as output that a run python file prints.
    private static StringBuilder output = new StringBuilder();
    //eventHandled is what we use to mark that the python file process is complete.
    private static TaskCompletionSource<bool> eventHandled = new TaskCompletionSource<bool>();




    public static RouteGroupBuilder MapDeepSeekEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("DeepSeek");
        group.MapPost("/generate", async ([FromBody] DeepSeekPromptDTO request) =>
        {
            try
            {

                // Ensure Python Script Exists
                var scriptPath = Path.Combine(ScriptsPath, RunScriptName);
                if (!File.Exists(scriptPath))
                {
                    return Results.BadRequest(new {
                        message = "Python Script Not Found",
                        detail = $"Script expected at: {scriptPath}",
                        statusCode = StatusCodes.Status500InternalServerError
                    });

                }
                

                if (!Directory.Exists(Path.Combine(ModelDirectory)))
                {
                    return Results.BadRequest(new {
                        message = "Model not downloaded. Please call /download-model first.",
                        solution = "Please call other endpoint first",
                        expectedPath = Path.GetFullPath(ModelDirectory)
                    });
                }

                // Validate Prompt
                if(string.IsNullOrEmpty(request.Prompt))
                {
                    return Results.BadRequest("Prompt Cannot Be Empty");
                }

                return await ExecutePythonScript(
                    scriptPath: scriptPath,
                    arguments: $"\"{ModelDirectory}\" \"{request.Prompt}\""
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: $"Generation Failed: {ex.Message}"
                );
            }
        })
        .WithName("GenerateText")
        .WithTags("DeepSeek")
        .WithDescription("Generates text using the locally downloaded model.")
        .Accepts<DeepSeekPromptDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/DownloadModel", async () => {
            try 
            {
                // Ensure Directory Exists
                Directory.CreateDirectory(ModelsRoot);

                if (Directory.Exists(ModelDirectory) &&
                    File.Exists(Path.Combine(ModelDirectory, "config.json")) &&
                    File.Exists(Path.Combine(ModelDirectory, "pytorch_model.bin"))) 
                {
                    return Results.Ok(new 
                    {
                        Message = "Model already downloaded",
                        path = Path.GetFullPath(ModelDirectory) 
                    });
                }

                // Ensure Python Script Exists
                var scriptPath = Path.Combine(ScriptsPath, DownloadScriptName);
                Console.WriteLine("Resolved path: " + scriptPath);
                if(!File.Exists(scriptPath))
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        detail: $"Python script not found at: {scriptPath}"
                    );
                }

                // Execute Download
                var results = await ExecutePythonScript(
                    scriptPath: scriptPath, 
                    arguments: $"\"{ModelDirectory}\""
                );
                if(results is not OkObjectResult okResults)
                {
                    return results;
                }

                // Verify Download Success
                if(!File.Exists(Path.Combine(ModelDirectory, "python_model.bin")))
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        detail: "Download Appeared to Complete but Model Files are missing"
                    );
                }

                if(results is OkObjectResult okResult) {
                    return Results.Ok(new {
                        status = "Success",
                        message = "Model Downloaded Succesfully",
                        path = Path.GetFullPath(ModelDirectory),
                        sixe = DirSize(new DirectoryInfo(ModelDirectory)),
                    });
                }
                return Results.Problem("Unexpected error: no return path hit.");

            } 
            catch (Exception ex) 
            {
                return Results.Problem
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Failed to download model.",
                    detail: ex.Message
                );
            }      
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
            Arguments = $"\"{scriptPath}\" {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };
        var output = new StringBuilder();

        using var process = new Process { StartInfo = processStartInfo };
        if(process == null)
        {
            return Results.Problem("Failed to Start Python process");
        }

        try
        {        
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    Console.WriteLine($"[PYTHON STDOUT] {args.Data}");
                    output.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    Console.Error.WriteLine($"[PYTHON STDERR] {args.Data}");
                }
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if(process.ExitCode != 0)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: $"Python Script failed with exit code: {process.ExitCode}\n" +
                            (string.IsNullOrEmpty(output.ToString()) ? "" : $"Standard output:\n{output.ToString()}")
                );
            }

            return Results.Ok(new {results=output.ToString()});
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

    private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            Console.WriteLine($"[PYTHON STDOUT] {outLine.Data}");
            output.AppendLine(outLine.Data);
        }
    }


    private static void ProcessExited(object sender, System.EventArgs e) {
            Console.WriteLine("Process exited whooo!");
            eventHandled.TrySetResult(true);
    }


    private static long DirSize(DirectoryInfo d)
    {
        long size = 0;
        FileInfo[] files = d.GetFiles();
        foreach (FileInfo file in files)
        {
            size += file.Length;
        }
        return size;
    }
}
