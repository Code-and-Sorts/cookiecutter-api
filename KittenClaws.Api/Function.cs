namespace KittenClaws.Api;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;
using Newtonsoft.Json;

public class Function : IHttpFunction
{
    private const string Endpoint = "kitties";
    private readonly IKittenClawsController _kittenClawsController;
    private readonly ILogger<Function> _logger;

    public Function(IKittenClawsController kittenClawsController, ILogger<Function> logger)
    {
        _kittenClawsController = kittenClawsController;
        _logger = logger;
    }

    private static bool IsValidEndpointPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 1 && segments[0] == Endpoint;
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

    private static async Task WriteJsonResponse(HttpResponse response, int statusCode, object body, CancellationToken ct = default)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        var json = JsonConvert.SerializeObject(body);
        await response.WriteAsync(json, ct);
    }

    public async Task HandleAsync(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var ct = context.RequestAborted;

        try
        {
            var path = request.Path.Value ?? string.Empty;

            var trimmedPath = path.TrimEnd('/');
            if (request.Method == "GET" && (trimmedPath == "/health" || trimmedPath.EndsWith("/health")))
            {
                await WriteJsonResponse(response, 200, new { status = "ok" }, ct);
                return;
            }

            if (!IsValidEndpointPath(path))
            {
                await WriteJsonResponse(response, 404, new { error = "Not found." }, ct);
                return;
            }

            switch (request.Method)
            {
                case "GET":
                {
                    var id = ParseId(path);
                    if (id != null)
                    {
                        _logger.LogInformation("GetKittenClaws processed a request.");
                        var result = await _kittenClawsController.GetAsync(id, ct);
                        await WriteJsonResponse(response, 200, result, ct);
                        return;
                    }
                    _logger.LogInformation("GetKittenClawsList processed a request.");
                    var results = await _kittenClawsController.GetListAsync(ct);
                    await WriteJsonResponse(response, 200, results, ct);
                    return;
                }

                case "POST":
                {
                    if (ParseId(path) != null)
                    {
                        await WriteJsonResponse(response, 400, new { error = "POST does not accept an item ID." }, ct);
                        return;
                    }
                    _logger.LogInformation("CreateKittenClaws processed a request.");
                    var created = await _kittenClawsController.CreateAsync(request.Body, ct);
                    await WriteJsonResponse(response, 201, created, ct);
                    return;
                }

                case "PATCH":
                {
                    var id = ParseId(path);
                    if (id == null)
                    {
                        await WriteJsonResponse(response, 400, new { error = "Missing item ID." }, ct);
                        return;
                    }
                    _logger.LogInformation("UpdateKittenClaws processed a request.");
                    var updated = await _kittenClawsController.UpdateAsync(id, request.Body, ct);
                    await WriteJsonResponse(response, 200, updated, ct);
                    return;
                }

                case "DELETE":
                {
                    var id = ParseId(path);
                    if (id == null)
                    {
                        await WriteJsonResponse(response, 400, new { error = "Missing item ID." }, ct);
                        return;
                    }
                    _logger.LogInformation("DeleteKittenClaws processed a request.");
                    await _kittenClawsController.DeleteAsync(id, ct);
                    await WriteJsonResponse(response, 200, new { message = $"KittenClaws with id {id} was deleted successfully." }, ct);
                    return;
                }

                default:
                    await WriteJsonResponse(response, 405, new { error = "Method not allowed." }, ct);
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Function -> HandleAsync method.");
            var errorResult = ErrorDetector.DetectError(ex);
            await WriteJsonResponse(response, errorResult.StatusCode ?? 500, errorResult.Value!, ct);
        }
    }
}
