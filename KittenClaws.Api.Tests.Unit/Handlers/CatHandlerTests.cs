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

public class CatHandlerTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly ICatController _mockCatController;
    private readonly CatHandler _handler;

    public CatHandlerTests()
    {
        _mockCatController = Substitute.For<ICatController>();
        _handler = new CatHandler(_mockCatController);
    }

    [Fact]
    public void Endpoint_IsCatEndpoint()
    {
        Assert.Equal("cats", _handler.Endpoint);
    }

    [Fact]
    public async Task HandleAsync_GetCat_ReturnsCamelCaseItem()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/cats/" + ItemId);
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat" }));

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockCat\",{Mocks.AuditJson()}}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_GetCatList_ReturnsItemsAndPassesLimit()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/cats", query: "?limit=2");
        _mockCatController.GetListAsync("2", Arg.Any<CancellationToken>())
            .Returns(new List<CatDto>
            {
                Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat1" }),
                Mocks.WithAuditFields(new CatDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" }, userId: null),
            });

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal($"[{{\"id\":\"{ItemId}\",\"name\":\"mockCat1\",{Mocks.AuditJson()}}},"
            + $"{{\"id\":\"5615ff05-3032-4459-88ad-b6a4c3e51ca0\",\"name\":\"mockCat2\",{Mocks.AuditJson(userId: null)}}}]", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_GetCatList_ReturnsEmptyArray()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/cats");
        _mockCatController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new List<CatDto>());

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("[]", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_CreateCat_ReturnsCreated()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateCatRequest { Name = "mockCat" }, "POST", "/cats");
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat" }));

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(201, httpContext.Response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockCat\",{Mocks.AuditJson()}}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PostWithId_ReturnsMethodNotAllowed()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateCatRequest { Name = "mockCat" }, "POST", "/cats/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
        await _mockCatController.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Fact]
    public async Task HandleAsync_UpdateCat_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext(new UpdateCatRequest { Name = "mockUpdatedCat" }, "PATCH", "/cats/" + ItemId);
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockUpdatedCat" }));

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockUpdatedCat\",{Mocks.AuditJson()}}}", Mocks.ReadResponseBody(httpContext));
        await _mockCatController.Received(1).UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DeleteCat_ReturnsDeleteMessage()
    {
        var httpContext = Mocks.CreateHttpContext("DELETE", "/cats/" + ItemId);
        _mockCatController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("Cat", ItemId));

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal($"{{\"message\":\"Cat with id {ItemId} was deleted successfully.\"}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PutById_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("PUT", "/cats/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PropagatesException_WhenControllerThrows()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/cats");
        _mockCatController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(httpContext, null));

        Assert.Equal("Mock exception", exception.Message);
    }
}
