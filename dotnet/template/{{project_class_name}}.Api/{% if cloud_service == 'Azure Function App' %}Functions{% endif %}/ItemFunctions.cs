namespace {{project_class_name}}.Api.Functions;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Utils;

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class ItemFunctions(IReadOnlyDictionary<string, IItemController> controllers, ILogger<ItemFunctions> logger)
{
    private readonly IReadOnlyDictionary<string, IItemController> _controllers = controllers;
    private readonly ILogger<ItemFunctions> _logger = logger;

    [Function("Health")]
    public Task<IActionResult> Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req, CancellationToken ct = default)
    {
        return Task.FromResult<IActionResult>(new OkObjectResult(new { status = "ok" }));
    }
{% for resource in resources %}
{%- if "get_by_id" in resource.operations %}

    [Function("Get{{ resource.name }}")]
    public async Task<IActionResult> Get{{ resource.name }}(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{{ resource.endpoint }}/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _controllers["{{ resource.container }}"].GetAsync(id, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Get{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "list" in resource.operations %}

    [Function("Get{{ resource.name }}List")]
    public async Task<IActionResult> Get{{ resource.name }}List(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{{ resource.endpoint }}")] HttpRequestData req, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _controllers["{{ resource.container }}"].GetListAsync(ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Get{{ resource.name }}List));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "create" in resource.operations %}

    [Function("Create{{ resource.name }}")]
    public async Task<IActionResult> Create{{ resource.name }}(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{{ resource.endpoint }}")] HttpRequestData req, CancellationToken ct = default)
    {
        try
        {
            var created = await _controllers["{{ resource.container }}"].CreateAsync(req.Body, ct);
            return new CreatedResult($"/api/{{ resource.endpoint }}", created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Create{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "update" in resource.operations %}

    [Function("Update{{ resource.name }}")]
    public async Task<IActionResult> Update{{ resource.name }}(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "{{ resource.endpoint }}/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _controllers["{{ resource.container }}"].UpdateAsync(id, req.Body, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Update{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "replace" in resource.operations %}

    [Function("Replace{{ resource.name }}")]
    public async Task<IActionResult> Replace{{ resource.name }}(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "{{ resource.endpoint }}/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            return new OkObjectResult(await _controllers["{{ resource.container }}"].ReplaceAsync(id, req.Body, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Replace{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "delete" in resource.operations %}

    [Function("Delete{{ resource.name }}")]
    public async Task<IActionResult> Delete{{ resource.name }}(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "{{ resource.endpoint }}/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        try
        {
            await _controllers["{{ resource.container }}"].DeleteAsync(id, ct);
            return new OkObjectResult(new DeleteOkObjectResult { Message = $"{{ resource.name }} with id {id} was deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Delete{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- endfor %}
}
