namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
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

public class CatFunctionsTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly ICatController _mockCatController;
    private readonly RecordingLogger<CatFunctions> _logger = new();
    private readonly CatFunctions _functions;

    public CatFunctionsTests()
    {
        _mockCatController = Substitute.For<ICatController>();
        _functions = new CatFunctions(_mockCatController, _logger);
    }

    private static Dictionary<string, string> IdPath(string id = ItemId) => new() { { "id", id } };

    private static APIGatewayProxyRequest OnRoute(APIGatewayProxyRequest request)
    {
        request.Path = request.PathParameters.TryGetValue("id", out var id) ? $"/cats/{id}" : "/cats";
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
    public async Task GetCat_ReturnsOk_WhenCatIsFound()
    {
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat" }));

        var response = await _functions.GetCat(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockCat\",{Mocks.AuditJson()}}}", response.Body);
    }

    [Fact]
    public async Task GetCat_ReturnsNotFound_WithoutLoggingAnError()
    {
        _mockCatController.GetAsync("not-a-uuid", Arg.Any<CancellationToken>()).Throws(new NotFoundException("Cat", "not-a-uuid"));

        var response = await _functions.GetCat(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath("not-a-uuid"))));

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Cat with id not-a-uuid was not found.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetCat_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetCat(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task GetCatList_ReturnsOk_WithItems()
    {
        _mockCatController.GetListAsync("1", Arg.Any<CancellationToken>())
            .Returns(new List<CatDto>
            {
                Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat1" }),
                Mocks.WithAuditFields(new CatDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" }, userId: null),
            });

        var response = await _functions.GetCatList(OnRoute(Mocks.CreateApiGatewayRequest("GET", queryStringParameters: new() { { "limit", "1" } })));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"[{{\"id\":\"{ItemId}\",\"name\":\"mockCat1\",{Mocks.AuditJson()}}},"
            + $"{{\"id\":\"5615ff05-3032-4459-88ad-b6a4c3e51ca0\",\"name\":\"mockCat2\",{Mocks.AuditJson(userId: null)}}}]", response.Body);
    }

    [Fact]
    public async Task GetCatList_ReturnsEmptyArray_WithoutQueryParameters()
    {
        _mockCatController.GetListAsync(null, Arg.Any<CancellationToken>()).Returns(new List<CatDto>());

        var response = await _functions.GetCatList(OnRoute(Mocks.CreateApiGatewayRequest(httpMethod: "GET")));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("[]", response.Body);
    }

    [Fact]
    public async Task GetCatList_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetCatList(OnRoute(Mocks.CreateApiGatewayRequest(httpMethod: "GET")));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task CreateCat_ReturnsCreated_WhenCatIsCreated()
    {
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat" }));

        var response = await _functions.CreateCat(OnRoute(Mocks.CreateApiGatewayRequest(new CreateCatRequest { Name = "mockCat" }, "POST")));

        Assert.Equal(201, response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockCat\",{Mocks.AuditJson()}}}", response.Body);
    }

    [Fact]
    public async Task CreateCat_ReturnsBadRequest_WhenBodyIsInvalid()
    {
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new BadRequestException("Request body must be valid JSON."));

        var response = await _functions.CreateCat(OnRoute(Mocks.CreateApiGatewayRequest("POST")));

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Request body must be valid JSON.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateCat_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.CreateCat(OnRoute(Mocks.CreateApiGatewayRequest(new CreateCatRequest { Name = "mockCat" }, "POST")));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task UpdateCat_ReturnsOk_WhenCatIsUpdated()
    {
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockUpdatedCat" }));

        var response = await _functions.UpdateCat(OnRoute(Mocks.CreateApiGatewayRequest(new UpdateCatRequest { Name = "mockUpdatedCat" }, "PATCH", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockUpdatedCat\",{Mocks.AuditJson()}}}", response.Body);
    }

    [Fact]
    public async Task UpdateCat_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.UpdateCat(OnRoute(Mocks.CreateApiGatewayRequest(new UpdateCatRequest { Name = "mockUpdatedCat" }, "PATCH", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task DeleteCat_ReturnsOk_WhenCatIsDeleted()
    {
        _mockCatController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("Cat", ItemId));

        var response = await _functions.DeleteCat(OnRoute(Mocks.CreateApiGatewayRequest("DELETE", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"{{\"message\":\"Cat with id {ItemId} was deleted successfully.\"}}", response.Body);
    }

    [Fact]
    public async Task DeleteCat_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.DeleteCat(OnRoute(Mocks.CreateApiGatewayRequest("DELETE", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task GetCatList_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS");
        request.Resource = "/cats";

        var response = await _functions.GetCatList(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetCatList_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("GET", IdPath());
        request.Path = "/cats/extra";

        var response = await _functions.GetCatList(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task GetCat_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/cats/{id}";

        var response = await _functions.GetCat(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetCat_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("GET");
        request.Path = "/cats/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.GetCat(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task CreateCat_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS");
        request.Resource = "/cats";

        var response = await _functions.CreateCat(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateCat_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("POST", IdPath());
        request.Path = "/cats/extra";

        var response = await _functions.CreateCat(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task UpdateCat_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/cats/{id}";

        var response = await _functions.UpdateCat(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task UpdateCat_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("PATCH");
        request.Path = "/cats/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.UpdateCat(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task DeleteCat_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/cats/{id}";

        var response = await _functions.DeleteCat(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task DeleteCat_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("DELETE");
        request.Path = "/cats/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.DeleteCat(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }
}
