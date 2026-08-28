namespace KittenClaws.Api;

using System;
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
    private readonly ICatController _catController;
    private readonly IDogController _dogController;
    private readonly ILogger<Function> _logger;

    public Function(
        ICatController catController,
        IDogController dogController,
        ILogger<Function> logger)
    {
        _catController = catController;
        _dogController = dogController;
        _logger = logger;
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

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var endpoint = segments.Length >= 1 ? segments[0] : string.Empty;
            var id = segments.Length >= 2 ? segments[1] : null;

            if (endpoint == "cats")
            {
                switch (request.Method)
                {
                    case "GET":
                    {
                        if (id != null)
                        {
                            var result = await _catController.GetAsync(id, ct);
                            await WriteJsonResponse(response, 200, result, ct);
                            return;
                        }
                        var results = await _catController.GetListAsync(ct);
                        await WriteJsonResponse(response, 200, results, ct);
                        return;
                    }

                    case "POST":
                    {
                        if (id != null)
                        {
                            await WriteJsonResponse(response, 400, new { error = "POST does not accept an item ID." }, ct);
                            return;
                        }
                        var created = await _catController.CreateAsync(request.Body, ct);
                        await WriteJsonResponse(response, 201, created, ct);
                        return;
                    }

                    case "PATCH":
                    {
                        if (id == null)
                        {
                            await WriteJsonResponse(response, 400, new { error = "Missing item ID." }, ct);
                            return;
                        }
                        var updated = await _catController.UpdateAsync(id, request.Body, ct);
                        await WriteJsonResponse(response, 200, updated, ct);
                        return;
                    }

                    case "PUT":
                    {
                        await WriteJsonResponse(response, 405, new { error = "Method not allowed." }, ct);
                        return;
                    }

                    case "DELETE":
                    {
                        if (id == null)
                        {
                            await WriteJsonResponse(response, 400, new { error = "Missing item ID." }, ct);
                            return;
                        }
                        await _catController.DeleteAsync(id, ct);
                        await WriteJsonResponse(response, 200, new { message = $"Cat with id {id} was deleted successfully." }, ct);
                        return;
                    }

                    default:
                        await WriteJsonResponse(response, 405, new { error = "Method not allowed." }, ct);
                        return;
                }
            }

            if (endpoint == "dogs")
            {
                switch (request.Method)
                {
                    case "GET":
                    {
                        if (id != null)
                        {
                            var result = await _dogController.GetAsync(id, ct);
                            await WriteJsonResponse(response, 200, result, ct);
                            return;
                        }
                        var results = await _dogController.GetListAsync(ct);
                        await WriteJsonResponse(response, 200, results, ct);
                        return;
                    }

                    case "POST":
                    {
                        if (id != null)
                        {
                            await WriteJsonResponse(response, 400, new { error = "POST does not accept an item ID." }, ct);
                            return;
                        }
                        var created = await _dogController.CreateAsync(request.Body, ct);
                        await WriteJsonResponse(response, 201, created, ct);
                        return;
                    }

                    case "PATCH":
                    {
                        await WriteJsonResponse(response, 405, new { error = "Method not allowed." }, ct);
                        return;
                    }

                    case "PUT":
                    {
                        if (id == null)
                        {
                            await WriteJsonResponse(response, 400, new { error = "Missing item ID." }, ct);
                            return;
                        }
                        var replaced = await _dogController.ReplaceAsync(id, request.Body, ct);
                        await WriteJsonResponse(response, 200, replaced, ct);
                        return;
                    }

                    case "DELETE":
                    {
                        if (id == null)
                        {
                            await WriteJsonResponse(response, 400, new { error = "Missing item ID." }, ct);
                            return;
                        }
                        await _dogController.DeleteAsync(id, ct);
                        await WriteJsonResponse(response, 200, new { message = $"Dog with id {id} was deleted successfully." }, ct);
                        return;
                    }

                    default:
                        await WriteJsonResponse(response, 405, new { error = "Method not allowed." }, ct);
                        return;
                }
            }

            await WriteJsonResponse(response, 404, new { error = "Not found." }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Function -> HandleAsync method.");
            var errorResult = ErrorDetector.DetectError(ex);
            await WriteJsonResponse(response, errorResult.StatusCode ?? 500, errorResult.Value!, ct);
        }
    }
}
