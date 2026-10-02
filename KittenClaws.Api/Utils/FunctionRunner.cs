namespace KittenClaws.Api.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;

public static class FunctionRunner
{
    // A function can be invoked without API Gateway, so each checks its own route first.
    public static async Task<APIGatewayProxyResponse> RunAsync<T>(
        APIGatewayProxyRequest request, string method, string route, ILogger logger, Func<CancellationToken, Task<T>> handle, int statusCode = 200)
        where T : notnull
    {
        var mismatch = RouteGuard.Check(request, method, route);
        if (mismatch != null)
        {
            return mismatch;
        }

        try
        {
            using var deadline = RequestDeadline.Start(CancellationToken.None);
            return ResponseHelper.WithStatus(statusCode, await handle(deadline.Token));
        }
        catch (Exception ex)
        {
            var (errorStatusCode, error) = ErrorDetector.Classify(ex, logger);
            return ResponseHelper.WithStatus(errorStatusCode, error);
        }
    }

    public static string Id(APIGatewayProxyRequest request) => request.PathParameters["id"];

    public static string? Query(APIGatewayProxyRequest request, string name) =>
        request.QueryStringParameters != null && request.QueryStringParameters.TryGetValue(name, out var value) ? value : null;

    public static Stream BodyStream(APIGatewayProxyRequest request) => new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? string.Empty));
}

public static class ResponseHelper
{
    public static APIGatewayProxyResponse Ok(object body) => WithStatus(200, body);

    public static APIGatewayProxyResponse WithStatus(int statusCode, object body) => new()
    {
        StatusCode = statusCode,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
        Body = Json.Serialize(body),
    };
}
