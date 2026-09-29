namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using KittenClaws.Api.Functions;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Dtos;
using NSubstitute.ExceptionExtensions;
using KittenClaws.Api.Utils;

public class DeleteKittenClawsTest
{
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly ILogger<DeleteKittenClaws> _mockLogger;
    private readonly DeleteKittenClaws _deleteKittenClawsFunction;

    public DeleteKittenClawsTest()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _mockLogger = Substitute.For<ILogger<DeleteKittenClaws>>();
        _deleteKittenClawsFunction = new DeleteKittenClaws(_mockKittenClawsController, _mockLogger);
    }

    [Fact]
    public async Task Delete_ReturnsDeleteResult_WhenKittenClawsIsDeleted()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var deleteKittenClawsDto = new KittenClawsDto { Id = kittenClawsId, Name = "mockKittenClaws" };
        var httpRequestData = Mocks.CreateHttpRequestData(deleteKittenClawsDto, "DELETE");

        _mockKittenClawsController.DeleteAsync(kittenClawsId, Arg.Any<CancellationToken>()).Returns(Task.FromResult(deleteKittenClawsDto));

        // Act
        var result = await _deleteKittenClawsFunction.Delete(httpRequestData, kittenClawsId);

        var deleteResult = Assert.IsType<OkObjectResult>(result);
        var errorResult = Assert.IsType<DeleteOkObjectResult>(deleteResult.Value);

        // Assert
        Assert.Equal(200, deleteResult.StatusCode);
        Assert.Equal("KittenClaws with id 0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c was deleted successfully.", errorResult.Message);
    }

    [Fact]
    public async Task Delete_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var deleteKittenClawsDto = new KittenClawsDto { Id = kittenClawsId, Name = "mockKittenClaws" };
        var httpRequestData = Mocks.CreateHttpRequestData(deleteKittenClawsDto, "DELETE");

        _mockKittenClawsController.DeleteAsync(kittenClawsId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _deleteKittenClawsFunction.Delete(httpRequestData, kittenClawsId);

        var objectResult = Assert.IsType<HttpResponseInit>(result);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);

        // Assert
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
