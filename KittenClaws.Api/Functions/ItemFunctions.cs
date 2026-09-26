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
    ICatController catController,
    IDogController dogController,
    ILogger<ItemFunctions> logger)
{
    private readonly ICatController _catController = catController;
    private readonly IDogController _dogController = dogController;
    private readonly ILogger<ItemFunctions> _logger = logger;

    [Function("Health")]
    public Task<IActionResult> Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req, CancellationToken ct = default)
    {
        return Task.FromResult<IActionResult>(new OkObjectResult(new { status = "ok" }));
    }


    [Function("GetCat")]
    public async Task<IActionResult> GetCat(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cats/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _catController.GetAsync(id, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetCat));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("GetCatList")]
    public async Task<IActionResult> GetCatList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cats")] HttpRequestData req, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _catController.GetListAsync(ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetCatList));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("CreateCat")]
    public async Task<IActionResult> CreateCat(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cats")] HttpRequestData req, CancellationToken ct = default)
    {
        try
        {
            var created = await _catController.CreateAsync(req.Body, ct);
            return new CreatedResult($"/api/cats", created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(CreateCat));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("UpdateCat")]
    public async Task<IActionResult> UpdateCat(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "cats/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _catController.UpdateAsync(id, req.Body, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(UpdateCat));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("DeleteCat")]
    public async Task<IActionResult> DeleteCat(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "cats/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            await _catController.DeleteAsync(id, ct);
            return new OkObjectResult(new DeleteOkObjectResult { Message = $"Cat with id {id} was deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(DeleteCat));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("GetDog")]
    public async Task<IActionResult> GetDog(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dogs/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _dogController.GetAsync(id, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetDog));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("GetDogList")]
    public async Task<IActionResult> GetDogList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dogs")] HttpRequestData req, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _dogController.GetListAsync(ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetDogList));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("CreateDog")]
    public async Task<IActionResult> CreateDog(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dogs")] HttpRequestData req, CancellationToken ct = default)
    {
        try
        {
            var created = await _dogController.CreateAsync(req.Body, ct);
            return new CreatedResult($"/api/dogs", created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(CreateDog));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("ReplaceDog")]
    public async Task<IActionResult> ReplaceDog(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "dogs/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _dogController.ReplaceAsync(id, req.Body, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(ReplaceDog));
            return ErrorDetector.DetectError(ex);
        }
    }

    [Function("DeleteDog")]
    public async Task<IActionResult> DeleteDog(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "dogs/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            await _dogController.DeleteAsync(id, ct);
            return new OkObjectResult(new DeleteOkObjectResult { Message = $"Dog with id {id} was deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(DeleteDog));
            return ErrorDetector.DetectError(ex);
        }
    }
}
