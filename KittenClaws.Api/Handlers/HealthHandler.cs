namespace KittenClaws.Api.Handlers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class HealthHandler : IResourceHandler
{
    public string Endpoint => "health";

    public async Task HandleAsync(HttpContext context, string? id)
    {
        var response = context.Response;
        var ct = context.RequestAborted;

        switch (context.Request.Method, id)
        {
            case ("GET", null):
                await response.WriteJsonAsync(StatusCodes.Status200OK, new { status = "ok" }, ct);
                return;
            case (_, null):
                await response.WriteErrorAsync(StatusCodes.Status405MethodNotAllowed, ErrorMessages.MethodNotAllowed, ct);
                return;
            default:
                await response.WriteErrorAsync(StatusCodes.Status404NotFound, ErrorMessages.NotFound, ct);
                return;
        }
    }
}
