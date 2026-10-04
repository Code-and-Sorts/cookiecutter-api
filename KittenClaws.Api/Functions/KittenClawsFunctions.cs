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

public class KittenClawsFunctions(IKittenClawsController controller, ILogger<KittenClawsFunctions> logger)
{
    private readonly IKittenClawsController _controller = controller;
    private readonly ILogger<KittenClawsFunctions> _logger = logger;

    [Function("GetKittenClaws")]
    public Task<IActionResult> GetKittenClaws(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kittenclaws/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.GetAsync(id, token));

    [Function("GetKittenClawsList")]
    public Task<IActionResult> GetKittenClawsList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kittenclaws")] HttpRequestData req, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.GetListAsync(HttpUtility.ParseQueryString(req.Url.Query)["limit"], token));

    [Function("CreateKittenClaws")]
    public Task<IActionResult> CreateKittenClaws(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "kittenclaws")] HttpRequestData req, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.CreateAsync(req.Body, UserIds.From(req), token), statusCode: 201);

    [Function("UpdateKittenClaws")]
    public Task<IActionResult> UpdateKittenClaws(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "kittenclaws/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.UpdateAsync(id, req.Body, UserIds.From(req), token));

    [Function("DeleteKittenClaws")]
    public Task<IActionResult> DeleteKittenClaws(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "kittenclaws/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.DeleteAsync(id, UserIds.From(req), token));
}
