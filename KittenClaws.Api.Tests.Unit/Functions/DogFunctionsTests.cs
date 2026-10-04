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

public class DogFunctionsTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IDogController _mockDogController;
    private readonly RecordingLogger<DogFunctions> _logger = new();
    private readonly DogFunctions _functions;

    public DogFunctionsTests()
    {
        _mockDogController = Substitute.For<IDogController>();
        _functions = new DogFunctions(_mockDogController, _logger);
    }

    private static Dictionary<string, string> IdPath(string id = ItemId) => new() { { "id", id } };

    private static APIGatewayProxyRequest OnRoute(APIGatewayProxyRequest request)
    {
        request.Path = request.PathParameters.TryGetValue("id", out var id) ? $"/dogs/{id}" : "/dogs";
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
    public async Task GetDog_ReturnsOk_WhenDogIsFound()
    {
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new DogDto { Id = ItemId, Name = "mockDog" }));

        var response = await _functions.GetDog(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockDog\",{Mocks.AuditJson()}}}", response.Body);
    }

    [Fact]
    public async Task GetDog_ReturnsNotFound_WithoutLoggingAnError()
    {
        _mockDogController.GetAsync("not-a-uuid", Arg.Any<CancellationToken>()).Throws(new NotFoundException("Dog", "not-a-uuid"));

        var response = await _functions.GetDog(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath("not-a-uuid"))));

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Dog with id not-a-uuid was not found.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetDog_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetDog(OnRoute(Mocks.CreateApiGatewayRequest("GET", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task GetDogList_ReturnsOk_WithItems()
    {
        _mockDogController.GetListAsync("1", Arg.Any<CancellationToken>())
            .Returns(new List<DogDto>
            {
                Mocks.WithAuditFields(new DogDto { Id = ItemId, Name = "mockDog1" }),
                Mocks.WithAuditFields(new DogDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" }, userId: null),
            });

        var response = await _functions.GetDogList(OnRoute(Mocks.CreateApiGatewayRequest("GET", queryStringParameters: new() { { "limit", "1" } })));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"[{{\"id\":\"{ItemId}\",\"name\":\"mockDog1\",{Mocks.AuditJson()}}},"
            + $"{{\"id\":\"5615ff05-3032-4459-88ad-b6a4c3e51ca0\",\"name\":\"mockDog2\",{Mocks.AuditJson(userId: null)}}}]", response.Body);
    }

    [Fact]
    public async Task GetDogList_ReturnsEmptyArray_WithoutQueryParameters()
    {
        _mockDogController.GetListAsync(null, Arg.Any<CancellationToken>()).Returns(new List<DogDto>());

        var response = await _functions.GetDogList(OnRoute(Mocks.CreateApiGatewayRequest(httpMethod: "GET")));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("[]", response.Body);
    }

    [Fact]
    public async Task GetDogList_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetDogList(OnRoute(Mocks.CreateApiGatewayRequest(httpMethod: "GET")));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task CreateDog_ReturnsCreated_WhenDogIsCreated()
    {
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new DogDto { Id = ItemId, Name = "mockDog" }));

        var response = await _functions.CreateDog(OnRoute(Mocks.CreateApiGatewayRequest(new CreateDogRequest { Name = "mockDog" }, "POST")));

        Assert.Equal(201, response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockDog\",{Mocks.AuditJson()}}}", response.Body);
    }

    [Fact]
    public async Task CreateDog_ReturnsBadRequest_WhenBodyIsInvalid()
    {
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new BadRequestException("Request body must be valid JSON."));

        var response = await _functions.CreateDog(OnRoute(Mocks.CreateApiGatewayRequest("POST")));

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Request body must be valid JSON.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateDog_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.CreateDog(OnRoute(Mocks.CreateApiGatewayRequest(new CreateDogRequest { Name = "mockDog" }, "POST")));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsOk_WhenDogIsReplaced()
    {
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new DogDto { Id = ItemId, Name = "mockReplacedDog" }));

        var response = await _functions.ReplaceDog(OnRoute(Mocks.CreateApiGatewayRequest(new ReplaceDogRequest { Name = "mockReplacedDog" }, "PUT", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockReplacedDog\",{Mocks.AuditJson()}}}", response.Body);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.ReplaceDog(OnRoute(Mocks.CreateApiGatewayRequest(new ReplaceDogRequest { Name = "mockReplacedDog" }, "PUT", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task DeleteDog_ReturnsOk_WhenDogIsDeleted()
    {
        _mockDogController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("Dog", ItemId));

        var response = await _functions.DeleteDog(OnRoute(Mocks.CreateApiGatewayRequest("DELETE", IdPath())));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"{{\"message\":\"Dog with id {ItemId} was deleted successfully.\"}}", response.Body);
    }

    [Fact]
    public async Task DeleteDog_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.DeleteDog(OnRoute(Mocks.CreateApiGatewayRequest("DELETE", IdPath())));

        AssertUnexpectedError(response);
    }

    [Fact]
    public async Task GetDogList_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS");
        request.Resource = "/dogs";

        var response = await _functions.GetDogList(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetDogList_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("GET", IdPath());
        request.Path = "/dogs/extra";

        var response = await _functions.GetDogList(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task GetDog_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/dogs/{id}";

        var response = await _functions.GetDog(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetDog_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("GET");
        request.Path = "/dogs/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.GetDog(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task CreateDog_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS");
        request.Resource = "/dogs";

        var response = await _functions.CreateDog(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateDog_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("POST", IdPath());
        request.Path = "/dogs/extra";

        var response = await _functions.CreateDog(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/dogs/{id}";

        var response = await _functions.ReplaceDog(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("PUT");
        request.Path = "/dogs/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.ReplaceDog(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }

    [Fact]
    public async Task DeleteDog_ReturnsMethodNotAllowed_ForAnotherMethodOnItsRoute()
    {
        var request = Mocks.CreateApiGatewayRequest("OPTIONS", IdPath());
        request.Resource = "/dogs/{id}";

        var response = await _functions.DeleteDog(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task DeleteDog_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest("DELETE");
        request.Path = "/dogs/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra";

        var response = await _functions.DeleteDog(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }
}
