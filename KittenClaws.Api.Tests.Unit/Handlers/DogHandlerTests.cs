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

public class DogHandlerTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IDogController _mockDogController;
    private readonly DogHandler _handler;

    public DogHandlerTests()
    {
        _mockDogController = Substitute.For<IDogController>();
        _handler = new DogHandler(_mockDogController);
    }

    [Fact]
    public void Endpoint_IsDogEndpoint()
    {
        Assert.Equal("dogs", _handler.Endpoint);
    }

    [Fact]
    public async Task HandleAsync_GetDog_ReturnsCamelCaseItem()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/dogs/" + ItemId);
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new DogDto { Id = ItemId, Name = "mockDog" }));

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockDog\",{Mocks.AuditJson()}}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_GetDogList_ReturnsItemsAndPassesLimit()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/dogs", query: "?limit=2");
        _mockDogController.GetListAsync("2", Arg.Any<CancellationToken>())
            .Returns(new List<DogDto>
            {
                Mocks.WithAuditFields(new DogDto { Id = ItemId, Name = "mockDog1" }),
                Mocks.WithAuditFields(new DogDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" }, userId: null),
            });

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal($"[{{\"id\":\"{ItemId}\",\"name\":\"mockDog1\",{Mocks.AuditJson()}}},"
            + $"{{\"id\":\"5615ff05-3032-4459-88ad-b6a4c3e51ca0\",\"name\":\"mockDog2\",{Mocks.AuditJson(userId: null)}}}]", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_GetDogList_ReturnsEmptyArray()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/dogs");
        _mockDogController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new List<DogDto>());

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("[]", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_CreateDog_ReturnsCreated()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateDogRequest { Name = "mockDog" }, "POST", "/dogs");
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new DogDto { Id = ItemId, Name = "mockDog" }));

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(201, httpContext.Response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockDog\",{Mocks.AuditJson()}}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PostWithId_ReturnsMethodNotAllowed()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateDogRequest { Name = "mockDog" }, "POST", "/dogs/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
        await _mockDogController.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Fact]
    public async Task HandleAsync_ReplaceDog_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext(new ReplaceDogRequest { Name = "mockReplacedDog" }, "PUT", "/dogs/" + ItemId);
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Mocks.WithAuditFields(new DogDto { Id = ItemId, Name = "mockReplacedDog" }));

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal($"{{\"id\":\"{ItemId}\",\"name\":\"mockReplacedDog\",{Mocks.AuditJson()}}}", Mocks.ReadResponseBody(httpContext));
        await _mockDogController.Received(1).ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DeleteDog_ReturnsDeleteMessage()
    {
        var httpContext = Mocks.CreateHttpContext("DELETE", "/dogs/" + ItemId);
        _mockDogController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("Dog", ItemId));

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal($"{{\"message\":\"Dog with id {ItemId} was deleted successfully.\"}}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PatchById_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("PATCH", "/dogs/" + ItemId);

        await _handler.HandleAsync(httpContext, ItemId);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PropagatesException_WhenControllerThrows()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/dogs");
        _mockDogController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(httpContext, null));

        Assert.Equal("Mock exception", exception.Message);
    }
}
