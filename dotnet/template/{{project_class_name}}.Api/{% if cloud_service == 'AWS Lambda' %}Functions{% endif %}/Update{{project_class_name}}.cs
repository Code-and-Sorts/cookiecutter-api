namespace {{project_class_name}}.Api.Functions;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Utils;

public class Update{{project_class_name}}(I{{project_class_name}}Controller {{project_lower_camel_name}}Controller, ILogger<Update{{project_class_name}}> logger)
{
    private readonly I{{project_class_name}}Controller _{{project_lower_camel_name}}Controller = {{project_lower_camel_name}}Controller;
    private readonly ILogger<Update{{project_class_name}}> _logger = logger;

    public async Task<APIGatewayProxyResponse> Patch(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(Update{{project_class_name}})} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? ""));
            var updated{{project_class_name}}Dto = await _{{project_lower_camel_name}}Controller.UpdateAsync(id, bodyStream);

            return ResponseHelper.Ok(updated{{project_class_name}}Dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Update{{project_class_name}}), nameof(Patch));

            return ErrorDetector.DetectError(ex);
        }
    }
}
