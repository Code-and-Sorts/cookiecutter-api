namespace KittenClaws.Api.Utils;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public static class FunctionRunner
{
    public static async Task<IActionResult> RunAsync<T>(ILogger logger, CancellationToken ct, Func<CancellationToken, Task<T>> handle, int statusCode = 200)
        where T : notnull
    {
        try
        {
            // The Cosmos DB gateway client can ignore cancellation, so stop waiting at the deadline.
            using var deadline = RequestDeadline.Start(ct);
            return JsonResponse(statusCode, await handle(deadline.Token).WaitAsync(deadline.Token));
        }
        catch (Exception ex)
        {
            var (errorStatusCode, error) = ErrorDetector.Classify(ex, logger, ct);
            return JsonResponse(errorStatusCode, error);
        }
    }

    public static IActionResult JsonResponse(int statusCode, object body) => new ContentResult
    {
        StatusCode = statusCode,
        ContentType = "application/json",
        Content = Json.Serialize(body),
    };
}
