namespace KittenClaws.Api.Utils;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public static class HttpResponseExtensions
{
    public static async Task WriteJsonAsync(this HttpResponse response, int statusCode, object body, CancellationToken ct = default)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        await response.WriteAsync(Json.Serialize(body), ct);
    }

    public static Task WriteErrorAsync(this HttpResponse response, int statusCode, string message, CancellationToken ct = default) =>
        response.WriteJsonAsync(statusCode, new BaseError { ErrorMessage = message }, ct);
}
