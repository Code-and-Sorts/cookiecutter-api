namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Utils;
using Microsoft.Azure.Functions.Worker.Http;

public class Create{{cookiecutter.project_class_name}}(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Create{{cookiecutter.project_class_name}}> logger)
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Create{{cookiecutter.project_class_name}}> _logger = logger;

    [Function("Create{{cookiecutter.project_class_name}}")]
    public async Task<IActionResult> Post(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{{cookiecutter.project_endpoint}}")] HttpRequestData create{{cookiecutter.project_class_name}}Request, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Create{{cookiecutter.project_class_name}})} processed a request.");

        try
        {
            var new{{cookiecutter.project_class_name}}Dto = await _{{cookiecutter.project_lower_camel_name}}Controller.CreateAsync(create{{cookiecutter.project_class_name}}Request.Body, ct);

            return new CreatedResult($"/api/{{cookiecutter.project_endpoint}}", new{{cookiecutter.project_class_name}}Dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Create{{cookiecutter.project_class_name}}), nameof(Post));

            return ErrorDetector.DetectError(ex);
        }
    }
}
