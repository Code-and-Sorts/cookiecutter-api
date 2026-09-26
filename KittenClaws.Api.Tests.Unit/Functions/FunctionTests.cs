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
        Substitute.For<IKittenClawsController>(),
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

public class KittenClawsFunctionTest
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly Function _function;

    public KittenClawsFunctionTest()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _function = new Function(
            _mockKittenClawsController,
            Substitute.For<ILogger<Function>>());
    }

    [Fact]
    public async Task HandleAsync_GetKittenClaws_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/kitties/" + ItemId);
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockKittenClawsController.Received(1).GetAsync(ItemId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetKittenClawsList_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/kitties");
        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<KittenClawsDto>
            {
                new() { Id = ItemId, Name = "mockKittenClaws1" },
                new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" },
            });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockKittenClawsController.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CreateKittenClaws_ReturnsCreated()
    {
        var httpContext = Mocks.CreateHttpContext(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST", "/kitties");
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(201, httpContext.Response.StatusCode);
        await _mockKittenClawsController.Received(1).CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UpdateKittenClaws_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext(new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" }, "PATCH", "/kitties/" + ItemId);
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockUpdatedKittenClaws" });

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        await _mockKittenClawsController.Received(1).UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DeleteKittenClaws_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("DELETE", "/kitties/" + ItemId);
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _function.HandleAsync(httpContext);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Contains("KittenClaws with id " + ItemId + " was deleted successfully.", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_PutById_ReturnsMethodNotAllowed_WhenDisabled()
    {
        var httpContext = Mocks.CreateHttpContext("PUT", "/kitties/" + ItemId);

        await _function.HandleAsync(httpContext);

        Assert.Equal(405, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/kitties");
        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        await _function.HandleAsync(httpContext);

        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Contains("Mock exception", Mocks.ReadResponseBody(httpContext));
    }
}

