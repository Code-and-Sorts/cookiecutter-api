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

public class CatFunctions(ICatController controller, ILogger<CatFunctions> logger)
{
    private readonly ICatController _controller = controller;
    private readonly ILogger<CatFunctions> _logger = logger;

    [Function("GetCat")]
    public Task<IActionResult> GetCat(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cats/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.GetAsync(id, token));

    [Function("GetCatList")]
    public Task<IActionResult> GetCatList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cats")] HttpRequestData req, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.GetListAsync(HttpUtility.ParseQueryString(req.Url.Query)["limit"], token));

    [Function("CreateCat")]
    public Task<IActionResult> CreateCat(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cats")] HttpRequestData req, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.CreateAsync(req.Body, UserIds.From(req), token), statusCode: 201);

    [Function("UpdateCat")]
    public Task<IActionResult> UpdateCat(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "cats/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.UpdateAsync(id, req.Body, UserIds.From(req), token));

    [Function("DeleteCat")]
    public Task<IActionResult> DeleteCat(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "cats/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.DeleteAsync(id, UserIds.From(req), token));
}
