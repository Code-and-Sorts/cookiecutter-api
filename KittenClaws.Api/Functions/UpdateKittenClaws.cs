namespace KittenClaws.Api.Functions;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class UpdateKittenClaws(IKittenClawsController kittenClawsController, ILogger<UpdateKittenClaws> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<UpdateKittenClaws> _logger = logger;

    public async Task<APIGatewayProxyResponse> Patch(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(UpdateKittenClaws)} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? ""));
            var updatedKittenClawsDto = await _kittenClawsController.UpdateAsync(id, bodyStream);

            return ResponseHelper.Ok(updatedKittenClawsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(UpdateKittenClaws), nameof(Patch));

            return ErrorDetector.DetectError(ex);
        }
    }
}
