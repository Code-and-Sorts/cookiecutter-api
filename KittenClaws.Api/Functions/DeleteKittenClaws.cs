namespace KittenClaws.Api.Functions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;
using Microsoft.Azure.Functions.Worker.Http;

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class DeleteKittenClaws(IKittenClawsController kittenClawsController, ILogger<DeleteKittenClaws> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<DeleteKittenClaws> _logger = logger;

    [Function("DeleteKittenClaws")]
    public async Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "kitties/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(DeleteKittenClaws)} processed a request.");

        try
        {
            await _kittenClawsController.DeleteAsync(id, ct);

            return new OkObjectResult(new DeleteOkObjectResult
            {
                Message = $"KittenClaws with id {id} was deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(DeleteKittenClaws), nameof(Delete));

            return ErrorDetector.DetectError(ex);
        }
    }
}
