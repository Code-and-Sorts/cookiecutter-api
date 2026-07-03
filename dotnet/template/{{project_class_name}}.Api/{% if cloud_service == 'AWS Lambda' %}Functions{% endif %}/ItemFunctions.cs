namespace {{project_class_name}}.Api.Functions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Utils;

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class ItemFunctions(
{%- for resource in resources %}
    I{{ resource.name }}Controller {{ resource.name | to_lower_camel }}Controller,
{%- endfor %}
    ILogger<ItemFunctions> logger)
{
{%- for resource in resources %}
    private readonly I{{ resource.name }}Controller _{{ resource.name | to_lower_camel }}Controller = {{ resource.name | to_lower_camel }}Controller;
{%- endfor %}
    private readonly ILogger<ItemFunctions> _logger = logger;

    private static Stream BodyStream(APIGatewayProxyRequest request) => new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? string.Empty));

    public APIGatewayProxyResponse Health(APIGatewayProxyRequest request) => ResponseHelper.Ok(new { status = "ok" });
{% for resource in resources %}
{%- if "get_by_id" in resource.operations %}

    public async Task<APIGatewayProxyResponse> Get{{ resource.name }}(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _{{ resource.name | to_lower_camel }}Controller.GetAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Get{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "list" in resource.operations %}

    public async Task<APIGatewayProxyResponse> Get{{ resource.name }}List(APIGatewayProxyRequest request)
    {
        try
        {
            return ResponseHelper.Ok(await _{{ resource.name | to_lower_camel }}Controller.GetListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Get{{ resource.name }}List));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "create" in resource.operations %}

    public async Task<APIGatewayProxyResponse> Create{{ resource.name }}(APIGatewayProxyRequest request)
    {
        try
        {
            return ResponseHelper.Created(await _{{ resource.name | to_lower_camel }}Controller.CreateAsync(BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Create{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "update" in resource.operations %}

    public async Task<APIGatewayProxyResponse> Update{{ resource.name }}(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _{{ resource.name | to_lower_camel }}Controller.UpdateAsync(id, BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Update{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "replace" in resource.operations %}

    public async Task<APIGatewayProxyResponse> Replace{{ resource.name }}(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _{{ resource.name | to_lower_camel }}Controller.ReplaceAsync(id, BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(Replace{{ resource.name }}));
            return ErrorDetector.DetectError(ex);
        }
    }
{%- endif %}
{%- if "delete" in resource.operations %}

    public async Task<APIGatewayProxyResponse> Delete{{ resource.name }}(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            await _{{ resource.name | to_lower_camel }}Controller.DeleteAsync(id);
            return ResponseHelper.Ok(new DeleteOkObjectResult { Message = $"{{ resource.name }} with id {id} was deleted successfully." });
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
