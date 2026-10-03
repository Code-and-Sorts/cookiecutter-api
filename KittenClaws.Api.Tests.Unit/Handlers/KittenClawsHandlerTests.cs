namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
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

public class KittenClawsHandlerTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly KittenClawsHandler _handler;

    public KittenClawsHandlerTests()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _handler = new KittenClawsHandler(_mockKittenClawsController);
    }

    [Fact]
    public void Endpoint_IsKittenClawsEndpoint()
    {
        Assert.Equal("kittenclaws", _handler.Endpoint);
    }

    [Fact]
    public async Task HandleAsync_GetKittenClaws_ReturnsCamelCaseItem()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/kittenclaws/" + ItemId);
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockKittenClaws\"}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_GetKittenClawsList_ReturnsItemsAndPassesLimit()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/kittenclaws", query: "?limit=2");
        _mockKittenClawsController.GetListAsync("2", Arg.Any<CancellationToken>())
            .Returns(new List<KittenClawsDto>
            {
                new() { Id = ItemId, Name = "mockKittenClaws1" },
                new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" },
            });

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal($"[{{\"id\":\"{ItemId}\",\"name\":\"mockKittenClaws1\"}},{{\"id\":\"5615ff05-3032-4459-88ad-b6a4c3e51ca0\",\"name\":\"mockKittenClaws2\"}}]", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_GetKittenClawsList_ReturnsEmptyArray()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/kittenclaws");
        _mockKittenClawsController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new List<KittenClawsDto>());

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("[]", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_CreateKittenClaws_ReturnsCreated()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST", "/kittenclaws");
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(201, httpContext.Response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockKittenClaws\"}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PostWithId_ReturnsMethodNotAllowed()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST", "/kittenclaws/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
        await _mockKittenClawsController.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Fact]
    public async Task HandleAsync_UpdateKittenClaws_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext(new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" }, "PATCH", "/kittenclaws/" + ItemId);
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockUpdatedKittenClaws" });

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockKittenClawsController.Received(1).UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DeleteKittenClaws_ReturnsDeleteMessage()
    {
        var httpContext = Mocks.CreateHttpContext("DELETE", "/kittenclaws/" + ItemId);
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("KittenClaws", ItemId));

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal($"{{\"message\":\"KittenClaws with id {ItemId} was deleted successfully.\"}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PutById_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("PUT", "/kittenclaws/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PropagatesException_WhenControllerThrows()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/kittenclaws");
        _mockKittenClawsController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(httpContext, null));

        Assert.Equal("Mock exception", exception.Message);
    }
}
