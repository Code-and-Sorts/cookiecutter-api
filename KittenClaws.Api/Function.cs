namespace KittenClaws.Api;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class Function : IHttpFunction
{
    private readonly IReadOnlyDictionary<string, IResourceHandler> _handlers;
    private readonly ILogger<Function> _logger;

    public Function(IEnumerable<IResourceHandler> handlers, ILogger<Function> logger)
    {
        _handlers = handlers.ToDictionary(handler => handler.Endpoint);
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var ct = context.RequestAborted;

        // Handlers read context.RequestAborted, so it now also ends at the deadline.
        using var deadline = RequestDeadline.Start(ct);
        context.RequestAborted = deadline.Token;

        try
        {
            var path = request.Path.Value ?? string.Empty;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length is 1 or 2 && _handlers.TryGetValue(segments[0], out var handler))
            {
                await handler.HandleAsync(context, segments.Length == 2 ? segments[1] : null);
                return;
            }

            await response.WriteErrorAsync(StatusCodes.Status404NotFound, ErrorMessages.NotFound, ct);
        }
        catch (Exception ex)
        {
            var (statusCode, error) = ErrorDetector.Classify(ex, _logger, ct);
            if (!ct.IsCancellationRequested)
            {
                await response.WriteJsonAsync(statusCode, error, ct);
            }
        }
    }
}
