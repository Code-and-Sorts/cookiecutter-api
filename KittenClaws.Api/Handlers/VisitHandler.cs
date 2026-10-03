namespace KittenClaws.Api.Handlers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class VisitHandler(IVisitController controller) : IResourceHandler
{
    private readonly IVisitController _controller = controller;

    public string Endpoint => "visits";

    public async Task HandleAsync(HttpContext context, string? id)
    {
        var request = context.Request;
        var response = context.Response;
        var ct = context.RequestAborted;

        switch (request.Method, id)
        {
            case ("GET", string itemId):
                await response.WriteJsonAsync(StatusCodes.Status200OK, await _controller.GetAsync(itemId, ct), ct);
                return;
            case ("POST", null):
                await response.WriteJsonAsync(StatusCodes.Status201Created, await _controller.CreateAsync(request.Body, UserIds.From(request), ct), ct);
                return;
            case ("PATCH", string itemId):
                await response.WriteJsonAsync(StatusCodes.Status200OK, await _controller.UpdateAsync(itemId, request.Body, UserIds.From(request), ct), ct);
                return;
            default:
                await response.WriteErrorAsync(StatusCodes.Status405MethodNotAllowed, ErrorMessages.MethodNotAllowed, ct);
                return;
        }
    }
}
