namespace KittenClaws.Api.Functions;

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class DeleteKittenClaws(IKittenClawsController kittenClawsController, ILogger<DeleteKittenClaws> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<DeleteKittenClaws> _logger = logger;

    public async Task<APIGatewayProxyResponse> Delete(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(DeleteKittenClaws)} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            await _kittenClawsController.DeleteAsync(id);

            return ResponseHelper.Ok(new DeleteOkObjectResult
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
