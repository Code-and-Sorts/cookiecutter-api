namespace KittenClaws.Api.Functions;

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class GetKittenClaws(IKittenClawsController kittenClawsController, ILogger<GetKittenClaws> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<GetKittenClaws> _logger = logger;

    public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(GetKittenClaws)} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            var kittenClaws = await _kittenClawsController.GetAsync(id);

            return ResponseHelper.Ok(kittenClaws);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(GetKittenClaws), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
