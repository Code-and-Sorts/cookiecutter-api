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

public class Create{{project_class_name}}(I{{project_class_name}}Controller {{project_lower_camel_name}}Controller, ILogger<Create{{project_class_name}}> logger)
{
    private readonly I{{project_class_name}}Controller _{{project_lower_camel_name}}Controller = {{project_lower_camel_name}}Controller;
    private readonly ILogger<Create{{project_class_name}}> _logger = logger;

    [Function("Create{{project_class_name}}")]
    public async Task<IActionResult> Post(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{{project_endpoint}}")] HttpRequestData create{{project_class_name}}Request, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Create{{project_class_name}})} processed a request.");

        try
        {
            var new{{project_class_name}}Dto = await _{{project_lower_camel_name}}Controller.CreateAsync(create{{project_class_name}}Request.Body, ct);

            return new CreatedResult($"/api/{{project_endpoint}}", new{{project_class_name}}Dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Create{{project_class_name}}), nameof(Post));

            return ErrorDetector.DetectError(ex);
        }
    }
}
