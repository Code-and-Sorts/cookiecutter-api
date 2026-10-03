namespace KittenClaws.Api.Handlers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class DogHandler(IDogController controller) : IResourceHandler
{
    private readonly IDogController _controller = controller;

    public string Endpoint => "dogs";

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
            case ("GET", null):
                await response.WriteJsonAsync(StatusCodes.Status200OK, await _controller.GetListAsync(request.Query["limit"].ToString(), ct), ct);
                return;
            case ("POST", null):
                await response.WriteJsonAsync(StatusCodes.Status201Created, await _controller.CreateAsync(request.Body, UserIds.From(request), ct), ct);
                return;
            case ("PUT", string itemId):
                await response.WriteJsonAsync(StatusCodes.Status200OK, await _controller.ReplaceAsync(itemId, request.Body, UserIds.From(request), ct), ct);
                return;
            case ("DELETE", string itemId):
                await response.WriteJsonAsync(StatusCodes.Status200OK, await _controller.DeleteAsync(itemId, UserIds.From(request), ct), ct);
                return;
            default:
                await response.WriteErrorAsync(StatusCodes.Status405MethodNotAllowed, ErrorMessages.MethodNotAllowed, ct);
                return;
        }
    }
}
