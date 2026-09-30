namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Functions;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;

public class KittenClawsFunctionsTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly RecordingLogger<KittenClawsFunctions> _logger = new();
    private readonly KittenClawsFunctions _functions;

    public KittenClawsFunctionsTests()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _functions = new KittenClawsFunctions(_mockKittenClawsController, _logger);
    }

    private static Dictionary<string, string> IdPath(string id = ItemId) => new() { { "id", id } };

    private static APIGatewayProxyRequest OnRoute(APIGatewayProxyRequest request)
    {
        request.Path = request.PathParameters.TryGetValue("id", out var id) ? $"/kittenclaws/{id}" : "/kittenclaws";
        return request;
    }

    private void AssertUnexpectedError(APIGatewayProxyResponse response)
    {
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        Assert.Equal("{\"errorMessage\":\"An unexpected error occurred.\"}", response.Body);
        Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception?.Message == "Mock exception");
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsOk_WhenKittenClawsIsFound()
    {
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        var response = await _functions.GetKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockKittenClaws\"}}", response.Body);
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsNotFound_WithoutLoggingAnError()
    {
        _mockKittenClawsController.GetAsync("not-a-uuid", Arg.Any<CancellationToken>()).Throws(new NotFoundException("KittenClaws", "not-a-uuid"));

        var response = await _functions.GetKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath("not-a-uuid"))));

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"KittenClaws with id not-a-uuid was not found.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsOk_WithItems()
    {
        _mockKittenClawsController.GetListAsync("1", Arg.Any<CancellationToken>())
            .Returns(new List<KittenClawsDto>
            {
                new() { Id = ItemId, Name = "mockKittenClaws1" },
                new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" },
            });

        var response = await _functions.GetKittenClawsList(OnRoute(Mocks.CreateApiGatewayRequest("GET", queryStringParameters: new() { { "limit", "1" } })));
        var responseDtos = JsonSerializer.Deserialize<List<KittenClawsDto>>(response.Body, Json.Options);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(2, responseDtos!.Count);
        Assert.StartsWith($"[{{\"id\":\"{ItemId}\",\"name\":\"mockKittenClaws1\"}}", response.Body);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsEmptyArray_WithoutQueryParameters()
    {
        _mockKittenClawsController.GetListAsync(null, Arg.Any<CancellationToken>()).Returns(new List<KittenClawsDto>());

        var response = await _functions.GetKittenClawsList(OnRoute(Mocks.CreateApiGatewayRequest(httpMethod: "GET")));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("[]", response.Body);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetKittenClawsList(OnRoute(Mocks.CreateApiGatewayRequest(httpMethod: "GET")));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsCreated_WhenKittenClawsIsCreated()
    {
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        var response = await _functions.CreateKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST")));
        var responseDto = JsonSerializer.Deserialize<KittenClawsDto>(response.Body, Json.Options);

        Assert.Equal(201, response.StatusCode);
        Assert.Equal(ItemId, responseDto!.Id);
        Assert.Equal("mockKittenClaws", responseDto.Name);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsBadRequest_WhenBodyIsInvalid()
    {
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new BadRequestException("Request body must be valid JSON."));

        var response = await _functions.CreateKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest("POST")));

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Request body must be valid JSON.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.CreateKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST")));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsOk_WhenKittenClawsIsUpdated()
    {
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockUpdatedKittenClaws" });

        var response = await _functions.UpdateKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest(new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" }, "PATCH", IdPath())));
        var responseDto = JsonSerializer.Deserialize<KittenClawsDto>(response.Body, Json.Options);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("mockUpdatedKittenClaws", responseDto!.Name);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.UpdateKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest(new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" }, "PATCH", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsOk_WhenKittenClawsIsDeleted()
    {
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("KittenClaws", ItemId));

        var response = await _functions.DeleteKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest("DELETE", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"{{\"message\":\"KittenClaws with id {ItemId} was deleted successfully.\"}}", response.Body);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.DeleteKittenClaws(OnRoute(Mocks.CreateApiGatewayRequest("DELETE", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS");
        request.Resource = "/kittenclaws";

        var response = await _functions.GetKittenClawsList(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("GET", IdPath());
        request.Path = "/kittenclaws/extra";

        var response = await _functions.GetKittenClawsList(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/kittenclaws/{id}";

        var response = await _functions.GetKittenClaws(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("GET");
        request.Path = "/kittenclaws/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.GetKittenClaws(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS");
        request.Resource = "/kittenclaws";

        var response = await _functions.CreateKittenClaws(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("POST", IdPath());
        request.Path = "/kittenclaws/extra";

        var response = await _functions.CreateKittenClaws(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/kittenclaws/{id}";

        var response = await _functions.UpdateKittenClaws(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("PATCH");
        request.Path = "/kittenclaws/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.UpdateKittenClaws(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/kittenclaws/{id}";

        var response = await _functions.DeleteKittenClaws(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("DELETE");
        request.Path = "/kittenclaws/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.DeleteKittenClaws(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }
}
