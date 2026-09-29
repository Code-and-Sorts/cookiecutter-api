using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace KittenClaws.Api.Functions;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class CreateKittenClaws(IKittenClawsController kittenClawsController, ILogger<CreateKittenClaws> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<CreateKittenClaws> _logger = logger;

    public async Task<APIGatewayProxyResponse> Post(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(CreateKittenClaws)} processed a request.");

        try
        {
            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? ""));
            var newKittenClawsDto = await _kittenClawsController.CreateAsync(bodyStream);

            return ResponseHelper.Created(newKittenClawsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(CreateKittenClaws), nameof(Post));

            return ErrorDetector.DetectError(ex);
        }
    }
}
