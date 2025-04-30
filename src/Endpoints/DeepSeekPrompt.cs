// src\Endpoints\LoginEndpoint.cs

using backend.Entities.Messages;
using System.Text;
using Microsoft.AspNetCore.Mvc; // For [FromBody]


namespace backend.Endpoints;

public static class DeepSeekEndpoints 
{
    public static RouteGroupBuilder MapDeepSeekEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("DeepSeek");
        group.MapPost("/generate", async ([FromBody] DeepSeekPromptDTO request, IHttpClientFactory httpClientFactory, HttpContext httpContext) =>
        {
            if (string.IsNullOrEmpty(request.Prompt))
            {
                return Results.BadRequest("Prompt cannot be empty.");
            }

            // Construct the JSON body for the Python microservice
            var requestBody = new // This is where parameters are set
            {
                prompt = request.Prompt,
                max_length = 256
            };

            // Convert to JSON
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var client  = httpClientFactory.CreateClient("HF");
            try
            {
                using var response = await client.PostAsync(
                    "generate",
                    content,
                    httpContext.RequestAborted
                );
                
                if(!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return Results.Problem(
                        detail: $"Hugging Face service returned error: {response.StatusCode}\n{body}",
                        statusCode: (int)response.StatusCode
                    );
                }

                // If successful, parse the JSON
                var responseJson = await response.Content.ReadAsStringAsync();
                return Results.Ok(responseJson);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: $"Failed calling Python service: {ex.Message}"
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

        group.MapPost("/PythonTest", async ([FromBody] DeepSeekPromptDTO request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return Results.BadRequest("Prompt cannot be empty.");

            var requestBody = new
            {
                prompt = request.Prompt
            };

            var jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("http://huggingface:5000/PythonServerTest", content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return Results.Problem(
                        detail: $"Python test endpoint error: {response.StatusCode}\n{body}",
                        statusCode: (int)response.StatusCode
                    );
                }

                // just forward whatever {"Throughput"} returns
                var responseJson = await response.Content.ReadAsStringAsync();
                return Results.Ok(responseJson);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: $"Failed calling Python test service: {ex.Message}"
                );
            }
        })
        .WithName("PythonTest")
        .WithTags("DeepSeek")
        .WithDescription("Smoke‑test the Python microservice endpoint.")
        .Accepts<DeepSeekPromptDTO>("application/json")
        .Produces<string>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);


        return group;
    }
}
