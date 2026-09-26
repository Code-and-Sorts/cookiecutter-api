namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using KittenClaws.Api;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;

public class FunctionTest
{
    private readonly Function _function = new(
        Substitute.For<ICatController>(),
        Substitute.For<IDogController>(),
        Substitute.For<ILogger<Function>>());

    [Fact]
    public async Task HandleAsync_Health_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/health");

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_UnknownEndpoint_ReturnsNotFound()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/unknown-endpoint");

        await _function.HandleAsync(httpContext);

        Assert.Equal(404, httpContext.Response.StatusCode);
    }
}

public class CatFunctionTest
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly ICatController _mockCatController;
    private readonly Function _function;

    public CatFunctionTest()
    {
        _mockCatController = Substitute.For<ICatController>();
        _function = new Function(
            _mockCatController,
            Substitute.For<IDogController>(),
            Substitute.For<ILogger<Function>>());
    }

    [Fact]
    public async Task HandleAsync_GetCat_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/cats/" + ItemId);
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(new CatDto { Id = ItemId, Name = "mockCat" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockCatController.Received(1).GetAsync(ItemId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetCatList_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/cats");
        _mockCatController.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CatDto>
            {
                new() { Id = ItemId, Name = "mockCat1" },
                new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" },
            });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockCatController.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CreateCat_ReturnsCreated()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateCatRequest { Name = "mockCat" }, "POST", "/cats");
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new CatDto { Id = ItemId, Name = "mockCat" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(201, httpContext.Response.StatusCode);
        await _mockCatController.Received(1).CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UpdateCat_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext(new UpdateCatRequest { Name = "mockUpdatedCat" }, "PATCH", "/cats/" + ItemId);
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new CatDto { Id = ItemId, Name = "mockUpdatedCat" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockCatController.Received(1).UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DeleteCat_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("DELETE", "/cats/" + ItemId);
        _mockCatController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Contains("Cat with id " + ItemId + " was deleted successfully.", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PutById_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("PUT", "/cats/" + ItemId);

        await _function.HandleAsync(httpContext);

        Assert.Equal(405, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/cats");
        _mockCatController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        await _function.HandleAsync(httpContext);

        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Contains("Mock exception", Mocks.ReadResponseBody(httpContext));
    }
}

public class DogFunctionTest
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IDogController _mockDogController;
    private readonly Function _function;

    public DogFunctionTest()
    {
        _mockDogController = Substitute.For<IDogController>();
        _function = new Function(
            Substitute.For<ICatController>(),
            _mockDogController,
            Substitute.For<ILogger<Function>>());
    }

    [Fact]
    public async Task HandleAsync_GetDog_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/dogs/" + ItemId);
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(new DogDto { Id = ItemId, Name = "mockDog" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockDogController.Received(1).GetAsync(ItemId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetDogList_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/dogs");
        _mockDogController.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DogDto>
            {
                new() { Id = ItemId, Name = "mockDog1" },
                new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" },
            });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockDogController.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CreateDog_ReturnsCreated()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateDogRequest { Name = "mockDog" }, "POST", "/dogs");
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new DogDto { Id = ItemId, Name = "mockDog" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(201, httpContext.Response.StatusCode);
        await _mockDogController.Received(1).CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ReplaceDog_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext(new ReplaceDogRequest { Name = "mockReplacedDog" }, "PUT", "/dogs/" + ItemId);
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new DogDto { Id = ItemId, Name = "mockReplacedDog" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockDogController.Received(1).ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DeleteDog_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("DELETE", "/dogs/" + ItemId);
        _mockDogController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Contains("Dog with id " + ItemId + " was deleted successfully.", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PatchById_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("PATCH", "/dogs/" + ItemId);

        await _function.HandleAsync(httpContext);

        Assert.Equal(405, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/dogs");
        _mockDogController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        await _function.HandleAsync(httpContext);

        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Contains("Mock exception", Mocks.ReadResponseBody(httpContext));
    }
}

