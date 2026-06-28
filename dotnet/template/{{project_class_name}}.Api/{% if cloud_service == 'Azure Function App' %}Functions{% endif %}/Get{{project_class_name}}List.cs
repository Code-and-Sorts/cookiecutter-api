namespace {{project_class_name}}.Api.Functions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Utils;

public class Get{{project_class_name}}List(I{{project_class_name}}Controller {{project_lower_camel_name}}Controller, ILogger<Get{{project_class_name}}List> logger)
{
    private readonly I{{project_class_name}}Controller _{{project_lower_camel_name}}Controller = {{project_lower_camel_name}}Controller;
    private readonly ILogger<Get{{project_class_name}}List> _logger = logger;

    [Function("Get{{project_class_name}}List")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{{project_endpoint}}")] CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Get{{project_class_name}}List)} processed a request.");

        try
        {
            var {{project_lower_camel_name}}List = await _{{project_lower_camel_name}}Controller.GetListAsync(ct);

            return new OkObjectResult({{project_lower_camel_name}}List);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Get{{project_class_name}}List), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
