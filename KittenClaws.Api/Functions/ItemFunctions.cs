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
    ICatController catController,
    IDogController dogController,
    ILogger<ItemFunctions> logger)
{
    private readonly ICatController _catController = catController;
    private readonly IDogController _dogController = dogController;
    private readonly ILogger<ItemFunctions> _logger = logger;

    private static Stream BodyStream(APIGatewayProxyRequest request) => new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? string.Empty));

    public APIGatewayProxyResponse Health(APIGatewayProxyRequest request) => ResponseHelper.Ok(new { status = "ok" });


    public async Task<APIGatewayProxyResponse> GetCat(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _catController.GetAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetCat));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> GetCatList(APIGatewayProxyRequest request)
    {
        try
        {
            return ResponseHelper.Ok(await _catController.GetListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetCatList));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> CreateCat(APIGatewayProxyRequest request)
    {
        try
        {
            return ResponseHelper.Created(await _catController.CreateAsync(BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(CreateCat));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> UpdateCat(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _catController.UpdateAsync(id, BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(UpdateCat));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> DeleteCat(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            await _catController.DeleteAsync(id);
            return ResponseHelper.Ok(new DeleteOkObjectResult { Message = $"Cat with id {id} was deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(DeleteCat));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> GetDog(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _dogController.GetAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetDog));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> GetDogList(APIGatewayProxyRequest request)
    {
        try
        {
            return ResponseHelper.Ok(await _dogController.GetListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(GetDogList));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> CreateDog(APIGatewayProxyRequest request)
    {
        try
        {
            return ResponseHelper.Created(await _dogController.CreateAsync(BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(CreateDog));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> ReplaceDog(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            return ResponseHelper.Ok(await _dogController.ReplaceAsync(id, BodyStream(request)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(ReplaceDog));
            return ErrorDetector.DetectError(ex);
        }
    }

    public async Task<APIGatewayProxyResponse> DeleteDog(APIGatewayProxyRequest request)
    {
        try
        {
            var id = request.PathParameters["id"];
            await _dogController.DeleteAsync(id);
            return ResponseHelper.Ok(new DeleteOkObjectResult { Message = $"Dog with id {id} was deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName}.", nameof(ItemFunctions), nameof(DeleteDog));
            return ErrorDetector.DetectError(ex);
        }
    }
}
