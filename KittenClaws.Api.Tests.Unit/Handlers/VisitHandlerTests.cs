namespace KittenClaws.Api.Tests.Unit;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Handlers;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;

public class VisitHandlerTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IVisitController _mockVisitController;
    private readonly VisitHandler _handler;

    public VisitHandlerTests()
    {
        _mockVisitController = Substitute.For<IVisitController>();
        _handler = new VisitHandler(_mockVisitController);
    }

    [Fact]
    public void Endpoint_IsVisitEndpoint()
    {
        Assert.Equal("visits", _handler.Endpoint);
    }

    [Fact]
    public async Task HandleAsync_GetVisit_ReturnsCamelCaseItem()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/visits/" + ItemId);
        _mockVisitController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(VisitSamples.Dto());

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        Assert.Equal(Json.Serialize(VisitSamples.Dto()), Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_CreateVisit_ReturnsCreated()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateVisitRequest(), "POST", "/visits");
        _mockVisitController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(VisitSamples.Dto());

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(201, httpContext.Response.StatusCode);
        Assert.Equal(Json.Serialize(VisitSamples.Dto()), Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PostWithId_ReturnsMethodNotAllowed()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateVisitRequest(), "POST", "/visits/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
        await _mockVisitController.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleAsync_UpdateVisit_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext(new UpdateVisitRequest(), "PATCH", "/visits/" + ItemId);
        _mockVisitController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(VisitSamples.Dto());

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockVisitController.Received(1).UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Get_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/visits");

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PutById_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("PUT", "/visits/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_DeleteById_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("DELETE", "/visits/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PropagatesException_WhenControllerThrows()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/visits/" + ItemId);
        _mockVisitController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(httpContext, ItemId));

        Assert.Equal("Mock exception", exception.Message);
    }
}
