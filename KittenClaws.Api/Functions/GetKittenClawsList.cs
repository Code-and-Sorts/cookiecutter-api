namespace KittenClaws.Api.Functions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class GetKittenClawsList(IKittenClawsController kittenClawsController, ILogger<GetKittenClawsList> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<GetKittenClawsList> _logger = logger;

    [Function("GetKittenClawsList")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kitties")] CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(GetKittenClawsList)} processed a request.");

        try
        {
            var kittenClawsList = await _kittenClawsController.GetListAsync(ct);

            return new OkObjectResult(kittenClawsList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(GetKittenClawsList), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
