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

public class VisitFunctionsTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IVisitController _mockVisitController;
    private readonly RecordingLogger<VisitFunctions> _logger = new();
    private readonly VisitFunctions _functions;

    public VisitFunctionsTests()
    {
        _mockVisitController = Substitute.For<IVisitController>();
        _functions = new VisitFunctions(_mockVisitController, _logger);
    }

    private static Dictionary<string, string> IdPath(string id = ItemId) => new() { { "id", id } };

    private static APIGatewayProxyRequest OnRoute(APIGatewayProxyRequest request)
    {
        request.Path = request.PathParameters.TryGetValue("id", out var id) ? $"/visits/{id}" : "/visits";
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
    public async Task GetVisit_ReturnsOk_WhenVisitIsFound()
    {
        _mockVisitController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(VisitSamples.Dto());

        var response = await _functions.GetVisit(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        Assert.Equal(Json.Serialize(VisitSamples.Dto()), response.Body);
    }

    [Fact]
    public async Task GetVisit_ReturnsNotFound_WithoutLoggingAnError()
    {
        _mockVisitController.GetAsync("not-a-uuid", Arg.Any<CancellationToken>()).Throws(new NotFoundException("Visit", "not-a-uuid"));

        var response = await _functions.GetVisit(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath("not-a-uuid"))));

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Visit with id not-a-uuid was not found.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetVisit_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockVisitController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetVisit(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task CreateVisit_ReturnsCreated_WhenVisitIsCreated()
    {
        _mockVisitController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(VisitSamples.Dto());

        var response = await _functions.CreateVisit(OnRoute(Mocks.CreateApiGatewayRequest(new CreateVisitRequest(), "POST")));

        Assert.Equal(201, response.StatusCode);
        Assert.Equal(Json.Serialize(VisitSamples.Dto()), response.Body);
    }

    [Fact]
    public async Task CreateVisit_ReturnsBadRequest_WhenBodyIsInvalid()
    {
        _mockVisitController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new BadRequestException("Request body must be valid JSON."));

        var response = await _functions.CreateVisit(OnRoute(Mocks.CreateApiGatewayRequest("POST")));

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Request body must be valid JSON.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateVisit_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockVisitController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.CreateVisit(OnRoute(Mocks.CreateApiGatewayRequest(new CreateVisitRequest(), "POST")));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task UpdateVisit_ReturnsOk_WhenVisitIsUpdated()
    {
        _mockVisitController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(VisitSamples.Dto());

        var response = await _functions.UpdateVisit(OnRoute(Mocks.CreateApiGatewayRequest(new UpdateVisitRequest(), "PATCH", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(Json.Serialize(VisitSamples.Dto()), response.Body);
    }

    [Fact]
    public async Task UpdateVisit_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockVisitController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.UpdateVisit(OnRoute(Mocks.CreateApiGatewayRequest(new UpdateVisitRequest(), "PATCH", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task GetVisit_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/visits/{id}";

        var response = await _functions.GetVisit(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetVisit_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("GET");
        request.Path = "/visits/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.GetVisit(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task CreateVisit_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS");
        request.Resource = "/visits";

        var response = await _functions.CreateVisit(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateVisit_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("POST", IdPath());
        request.Path = "/visits/extra";

        var response = await _functions.CreateVisit(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task UpdateVisit_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/visits/{id}";

        var response = await _functions.UpdateVisit(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task UpdateVisit_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("PATCH");
        request.Path = "/visits/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.UpdateVisit(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }
}
