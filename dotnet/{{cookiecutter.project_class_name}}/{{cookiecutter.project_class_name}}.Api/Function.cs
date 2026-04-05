namespace {{cookiecutter.project_class_name}}.Api;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Utils;
using Newtonsoft.Json;

public class Function : IHttpFunction
{
    private const string Endpoint = "{{cookiecutter.project_endpoint}}";
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Function> _logger;

    public Function(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Function> logger)
    {
        _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
        _logger = logger;
    }

    private static string? ParseId(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 && segments[0] == Endpoint)
        {
            return segments[1];
        }
        return null;
    }

    private static async Task WriteJsonResponse(HttpResponse response, int statusCode, object body)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        var json = JsonConvert.SerializeObject(body);
        await response.WriteAsync(json);
    }

    public async Task HandleAsync(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var ct = context.RequestAborted;

        try
        {
            switch (request.Method)
            {
                case "GET":
                {
                    var id = ParseId(request.Path);
                    if (id != null)
                    {
                        _logger.LogInformation("Get{{cookiecutter.project_class_name}} processed a request.");
                        var result = await _{{cookiecutter.project_lower_camel_name}}Controller.GetAsync(id, ct);
                        await WriteJsonResponse(response, 200, result);
                        return;
                    }
                    _logger.LogInformation("Get{{cookiecutter.project_class_name}}List processed a request.");
                    var results = await _{{cookiecutter.project_lower_camel_name}}Controller.GetListAsync(ct);
                    await WriteJsonResponse(response, 200, results);
                    return;
                }

                case "POST":
                {
                    _logger.LogInformation("Create{{cookiecutter.project_class_name}} processed a request.");
                    var created = await _{{cookiecutter.project_lower_camel_name}}Controller.CreateAsync(request.Body, ct);
                    await WriteJsonResponse(response, 201, created);
                    return;
                }

                case "PATCH":
                {
                    var id = ParseId(request.Path);
                    if (id == null)
                    {
                        await WriteJsonResponse(response, 400, new { error = "Missing item ID." });
                        return;
                    }
                    _logger.LogInformation("Update{{cookiecutter.project_class_name}} processed a request.");
                    var updated = await _{{cookiecutter.project_lower_camel_name}}Controller.UpdateAsync(id, request.Body, ct);
                    await WriteJsonResponse(response, 200, updated);
                    return;
                }

                case "DELETE":
                {
                    var id = ParseId(request.Path);
                    if (id == null)
                    {
                        await WriteJsonResponse(response, 400, new { error = "Missing item ID." });
                        return;
                    }
                    _logger.LogInformation("Delete{{cookiecutter.project_class_name}} processed a request.");
                    await _{{cookiecutter.project_lower_camel_name}}Controller.DeleteAsync(id, ct);
                    await WriteJsonResponse(response, 200, new { message = $"{{cookiecutter.project_class_name}} with id {id} was deleted successfully." });
                    return;
                }

                default:
                    await WriteJsonResponse(response, 405, new { error = "Method not allowed." });
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Function -> HandleAsync method.");
            var errorResult = ErrorDetector.DetectError(ex);
            await WriteJsonResponse(response, errorResult.StatusCode ?? 500, errorResult.Value!);
        }
    }
}
