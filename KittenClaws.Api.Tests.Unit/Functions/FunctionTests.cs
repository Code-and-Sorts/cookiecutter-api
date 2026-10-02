namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using KittenClaws.Api;
using KittenClaws.Api.Handlers;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class FunctionTests
{
    private readonly IResourceHandler _mockHandler;
    private readonly RecordingLogger<Function> _logger = new();
    private readonly Function _function;

    public FunctionTests()
    {
        _mockHandler = Substitute.For<IResourceHandler>();
        _mockHandler.Endpoint.Returns("things");
        _function = new Function([_mockHandler], _logger);
    }

    [Theory]
    [InlineData("/unknown-endpoint")]
    [InlineData("/")]
    [InlineData("/things/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c/extra")]
    [InlineData("/prefix/things")]
    public async Task HandleAsync_UnknownPath_ReturnsNotFound(string path)
    {
        var httpContext = Mocks.CreateHttpContext("GET", path);

        await _function.HandleAsync(httpContext);

        Assert.Equal(404, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", Mocks.ReadResponseBody(httpContext));
        await _mockHandler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_CollectionPath_DispatchesToHandlerWithoutId()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/things");

        await _function.HandleAsync(httpContext);

        await _mockHandler.Received(1).HandleAsync(httpContext, null);
    }

    [Fact]
    public async Task HandleAsync_ItemPath_DispatchesToHandlerWithId()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/things/0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c");

        await _function.HandleAsync(httpContext);

        await _mockHandler.Received(1).HandleAsync(httpContext, "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c");
    }

    [Fact]
    public async Task HandleAsync_ReturnsGenericErrorAndLogs_WhenHandlerThrows()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/things");
        _mockHandler.HandleAsync(Arg.Any<HttpContext>(), Arg.Any<string?>()).Throws(new Exception("Mock exception"));

        await _function.HandleAsync(httpContext);

        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"An unexpected error occurred.\"}", Mocks.ReadResponseBody(httpContext));
        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("Mock exception", entry.Exception?.Message);
    }

    [Fact]
    public async Task HandleAsync_DoesNotLogAnError_WhenTheClientCancels()
    {
        using var aborted = new System.Threading.CancellationTokenSource();
        var httpContext = Mocks.CreateHttpContext("GET", "/things");
        httpContext.RequestAborted = aborted.Token;
        aborted.Cancel();
        _mockHandler.HandleAsync(Arg.Any<HttpContext>(), Arg.Any<string?>()).Throws(new OperationCanceledException());

        await _function.HandleAsync(httpContext);

        Assert.DoesNotContain(_logger.Entries, entry => entry.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task HandleAsync_GivesHandlersATokenBoundedByTheRequestDeadline()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/things");
        var requestAborted = httpContext.RequestAborted;
        System.Threading.CancellationToken handlerToken = default;
        _mockHandler.HandleAsync(Arg.Do<HttpContext>(context => handlerToken = context.RequestAborted), Arg.Any<string?>()).Returns(Task.CompletedTask);

        await _function.HandleAsync(httpContext);

        Assert.True(handlerToken.CanBeCanceled);
        Assert.NotEqual(requestAborted, handlerToken);
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequestWithoutLogging_WhenBodyIsMalformed()
    {
        var httpContext = Mocks.CreateHttpContext("POST", "/things", body: "{not json");
        _mockHandler.HandleAsync(Arg.Any<HttpContext>(), Arg.Any<string?>()).Throws(new BadRequestException("Request body must be valid JSON."));

        await _function.HandleAsync(httpContext);

        Assert.Equal(400, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Request body must be valid JSON.\"}", Mocks.ReadResponseBody(httpContext));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFoundWithoutLogging_WhenItemIsMissing()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/things/abc");
        _mockHandler.HandleAsync(Arg.Any<HttpContext>(), Arg.Any<string?>()).Throws(new NotFoundException("Thing", "abc"));

        await _function.HandleAsync(httpContext);

        Assert.Equal(404, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Thing with id abc was not found.\"}", Mocks.ReadResponseBody(httpContext));
        Assert.Empty(_logger.Entries);
    }

    [Theory]
    [InlineData("/cats")]
    [InlineData("/dogs")]
    [InlineData("/health")]
    public async Task HandleAsync_ResourceEndpoint_IsRoutedToItsHandler(string path)
    {
        var function = new Function(
        [
            new CatHandler(Substitute.For<ICatController>()),
            new DogHandler(Substitute.For<IDogController>()),
            new HealthHandler(),
        ], _logger);
        var httpContext = Mocks.CreateHttpContext("OPTIONS", path);

        await function.HandleAsync(httpContext);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
    }
}
