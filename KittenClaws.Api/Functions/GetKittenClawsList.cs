namespace KittenClaws.Api.Functions;

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class GetKittenClawsList(IKittenClawsController kittenClawsController, ILogger<GetKittenClawsList> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<GetKittenClawsList> _logger = logger;

    public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(GetKittenClawsList)} processed a request.");

        try
        {
            var kittenClawsList = await _kittenClawsController.GetListAsync();

            return ResponseHelper.Ok(kittenClawsList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(GetKittenClawsList), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
