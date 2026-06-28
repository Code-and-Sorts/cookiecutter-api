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

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class Delete{{project_class_name}}(I{{project_class_name}}Controller {{project_lower_camel_name}}Controller, ILogger<Delete{{project_class_name}}> logger)
{
    private readonly I{{project_class_name}}Controller _{{project_lower_camel_name}}Controller = {{project_lower_camel_name}}Controller;
    private readonly ILogger<Delete{{project_class_name}}> _logger = logger;

    [Function("Delete{{project_class_name}}")]
    public async Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "{{project_endpoint}}/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Delete{{project_class_name}})} processed a request.");

        try
        {
            await _{{project_lower_camel_name}}Controller.DeleteAsync(id, ct);

            return new OkObjectResult(new DeleteOkObjectResult
            {
                Message = $"{{project_class_name}} with id {id} was deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Delete{{project_class_name}}), nameof(Delete));

            return ErrorDetector.DetectError(ex);
        }
    }
}
