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

public class CreateKittenClaws(IKittenClawsController kittenClawsController, ILogger<CreateKittenClaws> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<CreateKittenClaws> _logger = logger;

    [Function("CreateKittenClaws")]
    public async Task<IActionResult> Post(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "kitties")] HttpRequestData createKittenClawsRequest, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(CreateKittenClaws)} processed a request.");

        try
        {
            var newKittenClawsDto = await _kittenClawsController.CreateAsync(createKittenClawsRequest.Body, ct);

            return new CreatedResult($"/api/kitties", newKittenClawsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(CreateKittenClaws), nameof(Post));

            return ErrorDetector.DetectError(ex);
        }
    }
}
