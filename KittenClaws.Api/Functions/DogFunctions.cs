namespace KittenClaws.Api.Functions;

using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class DogFunctions(IDogController controller, ILogger<DogFunctions> logger)
{
    private readonly IDogController _controller = controller;
    private readonly ILogger<DogFunctions> _logger = logger;

    [Function("GetDog")]
    public Task<IActionResult> GetDog(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dogs/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.GetAsync(id, token));

    [Function("GetDogList")]
    public Task<IActionResult> GetDogList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dogs")] HttpRequestData req, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.GetListAsync(HttpUtility.ParseQueryString(req.Url.Query)["limit"], token));

    [Function("CreateDog")]
    public Task<IActionResult> CreateDog(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dogs")] HttpRequestData req, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.CreateAsync(req.Body, UserIds.From(req), token), statusCode: 201);

    [Function("ReplaceDog")]
    public Task<IActionResult> ReplaceDog(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "dogs/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.ReplaceAsync(id, req.Body, UserIds.From(req), token));

    [Function("DeleteDog")]
    public Task<IActionResult> DeleteDog(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "dogs/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.DeleteAsync(id, UserIds.From(req), token));
}
