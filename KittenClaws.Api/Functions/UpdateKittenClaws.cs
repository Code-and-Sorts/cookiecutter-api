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

public class UpdateKittenClaws(IKittenClawsController kittenClawsController, ILogger<UpdateKittenClaws> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<UpdateKittenClaws> _logger = logger;

    [Function("UpdateKittenClaws")]
    public async Task<IActionResult> Patch(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "kitties/{id}")] HttpRequestData updateKittenClawsRequest, string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(UpdateKittenClaws)} processed a request.");

        try
        {
            var updatedKittenClawsDto = await _kittenClawsController.UpdateAsync(id, updateKittenClawsRequest.Body, ct);

            return new OkObjectResult(updatedKittenClawsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(UpdateKittenClaws), nameof(Patch));

            return ErrorDetector.DetectError(ex);
        }
    }
}
