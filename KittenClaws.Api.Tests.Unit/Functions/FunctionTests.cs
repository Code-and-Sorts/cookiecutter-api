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
using Newtonsoft.Json;

public class FunctionTest
{
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly ILogger<Function> _mockLogger;
    private readonly Function _function;

    public FunctionTest()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _mockLogger = Substitute.For<ILogger<Function>>();
        _function = new Function(_mockKittenClawsController, _mockLogger);
    }

    [Fact]
    public async Task HandleAsync_Get_ReturnsResult()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var kittenClawsDto = new KittenClawsDto { Id = id, Name = "mockKittenClaws" };
        var httpContext = Mocks.CreateHttpContext("GET", "/kitties/" + id);

        _mockKittenClawsController.GetAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(kittenClawsDto));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_GetList_ReturnsResults()
    {
        // Arrange
        var kittenClawsList = new List<KittenClawsDto>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" }
        };
        var httpContext = Mocks.CreateHttpContext("GET", "/kitties");

        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<KittenClawsDto>>(kittenClawsList));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_Post_ReturnsCreatedResult()
    {
        // Arrange
        var createRequest = new CreateKittenClawsRequest { Name = "mockKittenClaws" };
        var newKittenClawsDto = new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws" };
        var httpContext = Mocks.CreateHttpContext(createRequest, "POST", "/kitties");

        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newKittenClawsDto));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(201, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_Patch_ReturnsUpdatedResult()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" };
        var updatedKittenClawsDto = new KittenClawsDto { Id = id, Name = "mockUpdatedKittenClaws" };
        var httpContext = Mocks.CreateHttpContext(updateRequest, "PATCH", "/kitties/" + id);

        _mockKittenClawsController.UpdateAsync(id, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(updatedKittenClawsDto));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_Delete_ReturnsSuccessResult()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var httpContext = Mocks.CreateHttpContext("DELETE", "/kitties/" + id);

        _mockKittenClawsController.DeleteAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var httpContext = Mocks.CreateHttpContext("GET", "/kitties/" + id);

        _mockKittenClawsController.GetAsync(id, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(500, httpContext.Response.StatusCode);
    }
}
