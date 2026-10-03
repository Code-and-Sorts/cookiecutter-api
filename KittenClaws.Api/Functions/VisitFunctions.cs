namespace KittenClaws.Api.Functions;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class VisitFunctions(IVisitController controller, ILogger<VisitFunctions> logger)
{
    private readonly IVisitController _controller = controller;
    private readonly ILogger<VisitFunctions> _logger = logger;

    [Function("GetVisit")]
    public Task<IActionResult> GetVisit(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "visits/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.GetAsync(id, token));

    [Function("CreateVisit")]
    public Task<IActionResult> CreateVisit(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "visits")] HttpRequestData req, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.CreateAsync(req.Body, UserIds.From(req), token), statusCode: 201);

    [Function("UpdateVisit")]
    public Task<IActionResult> UpdateVisit(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "visits/{id}")] HttpRequestData req, string id, CancellationToken ct = default) =>
        FunctionRunner.RunAsync(_logger, ct, token => _controller.UpdateAsync(id, req.Body, UserIds.From(req), token));
}
