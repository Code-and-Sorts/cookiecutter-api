using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Utils;

public class Create{{cookiecutter.project_class_name}}(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Create{{cookiecutter.project_class_name}}> logger)
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Create{{cookiecutter.project_class_name}}> _logger = logger;

    public async Task<APIGatewayProxyResponse> Post(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(Create{{cookiecutter.project_class_name}})} processed a request.");

        try
        {
            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? ""));
            var new{{cookiecutter.project_class_name}}Dto = await _{{cookiecutter.project_lower_camel_name}}Controller.CreateAsync(bodyStream);

            return ResponseHelper.Created(new{{cookiecutter.project_class_name}}Dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Create{{cookiecutter.project_class_name}}), nameof(Post));

            return ErrorDetector.DetectError(ex);
        }
    }
}
