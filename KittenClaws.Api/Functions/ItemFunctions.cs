namespace KittenClaws.Api.Functions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class ItemFunctions(
    IKittenClawsController kittenClawsController,
    ILogger<ItemFunctions> logger)
{
    private readonly IKittenClawsController _kittenClawsController = kittenClawsController;
    private readonly ILogger<ItemFunctions> _logger = logger;

    private static Stream BodyStream(APIGatewayProxyRequest request) => new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? string.Empty));

    public APIGatewayProxyResponse Health(APIGatewayProxyRequest request) => ResponseHelper.Ok(new { status = "ok" });


    public async Task<APIGatewayProxyResponse> GetKittenClaws(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _kittenClawsController.GetAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetKittenClaws));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> GetKittenClawsList(APIGatewayProxyRequest request)
    {
        try
        {
            return ResponseHelper.Ok(await _kittenClawsController.GetListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetKittenClawsList));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> CreateKittenClaws(APIGatewayProxyRequest request)
    {
        try
        {
            return ResponseHelper.Created(await _kittenClawsController.CreateAsync(BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(CreateKittenClaws));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> UpdateKittenClaws(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _kittenClawsController.UpdateAsync(id, BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(UpdateKittenClaws));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> DeleteKittenClaws(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            await _kittenClawsController.DeleteAsync(id);
            return ResponseHelper.Ok(new DeleteOkObjectResult { Message = $"KittenClaws with id {id} was deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(DeleteKittenClaws));
            return ErrorDetector.DetectError(ex);
        }
    }
}
