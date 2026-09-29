namespace KittenClaws.Api.Functions;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class ItemFunctions(
    IKittenClawsController kittenClawsController,
    ILogger<ItemFunctions> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<ItemFunctions> _logger = logger;

    [Function("Health")]
    public Task<IActionResult> Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req, CancellationToken ct = default)
    {
        return Task.FromResult<IActionResult>(new OkObjectResult(new { status = "ok" }));
    }


    [Function("GetKittenClaws")]
    public async Task<IActionResult> GetKittenClaws(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kitties/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _kittenClawsController.GetAsync(id, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetKittenClaws));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("GetKittenClawsList")]
    public async Task<IActionResult> GetKittenClawsList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kitties")] HttpRequestData req, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _kittenClawsController.GetListAsync(ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetKittenClawsList));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("CreateKittenClaws")]
    public async Task<IActionResult> CreateKittenClaws(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "kitties")] HttpRequestData req, CancellationToken ct = default)
    {
        try
        {
            var created = await _kittenClawsController.CreateAsync(req.Body, ct);
            return new CreatedResult($"/api/kitties", created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(CreateKittenClaws));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("UpdateKittenClaws")]
    public async Task<IActionResult> UpdateKittenClaws(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "kitties/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _kittenClawsController.UpdateAsync(id, req.Body, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(UpdateKittenClaws));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("DeleteKittenClaws")]
    public async Task<IActionResult> DeleteKittenClaws(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "kitties/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            await _kittenClawsController.DeleteAsync(id, ct);
            return new OkObjectResult(new DeleteOkObjectResult { Message = $"KittenClaws with id {id} was deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(DeleteKittenClaws));
            return ErrorDetector.DetectError(ex);
        }
    }
}
