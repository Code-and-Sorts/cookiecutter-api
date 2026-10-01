namespace {{project_class_name}}.Api.Handlers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Utils;

public class OpenApiHandler : IResourceHandler
{
    public string Endpoint => "openapi.json";

    public async Task HandleAsync(HttpContext context, string? id)
    {
        var response = context.Response;
        var ct = context.RequestAborted;

        switch (context.Request.Method, id)
        {
            case ("GET", null):
                response.StatusCode = StatusCodes.Status200OK;
                response.ContentType = "application/json";
                await response.WriteAsync(OpenApiDocument.Json, ct);
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
