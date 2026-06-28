namespace {{project_class_name}}.Api.Functions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Utils;
using Microsoft.Azure.Functions.Worker.Http;

public class Get{{project_class_name}}(I{{project_class_name}}Controller {{project_lower_camel_name}}Controller, ILogger<Get{{project_class_name}}> logger)
{
    private readonly I{{project_class_name}}Controller _{{project_lower_camel_name}}Controller = {{project_lower_camel_name}}Controller;
    private readonly ILogger<Get{{project_class_name}}> _logger = logger;

    [Function("Get{{project_class_name}}")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{{project_endpoint}}/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Get{{project_class_name}})} processed a request.");

        try
        {
            var {{project_lower_camel_name}} = await _{{project_lower_camel_name}}Controller.GetAsync(id, ct);

            return new OkObjectResult({{project_lower_camel_name}});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Get{{project_class_name}}), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
