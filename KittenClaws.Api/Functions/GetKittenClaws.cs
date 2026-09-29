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

public class GetKittenClaws(IKittenClawsController kittenClawsController, ILogger<GetKittenClaws> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<GetKittenClaws> _logger = logger;

    [Function("GetKittenClaws")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kitties/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(GetKittenClaws)} processed a request.");

        try
        {
            var kittenClaws = await _kittenClawsController.GetAsync(id, ct);

            return new OkObjectResult(kittenClaws);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(GetKittenClaws), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
