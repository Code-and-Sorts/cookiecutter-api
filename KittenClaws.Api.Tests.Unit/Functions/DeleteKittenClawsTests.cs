namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using KittenClaws.Api.Functions;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Utils;
using Newtonsoft.Json;

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
        var request = Mocks.CreateApiGatewayRequest("DELETE", new Dictionary<string, string> { { "id", kittenClawsId } });

        _mockKittenClawsController.DeleteAsync(kittenClawsId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await _deleteKittenClawsFunction.Delete(request);
        var deleteResult = JsonConvert.DeserializeObject<DeleteOkObjectResult>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("KittenClaws with id 0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c was deleted successfully.", deleteResult!.Message);
    }

    [Fact]
    public async Task Delete_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var request = Mocks.CreateApiGatewayRequest("DELETE", new Dictionary<string, string> { { "id", kittenClawsId } });

        _mockKittenClawsController.DeleteAsync(kittenClawsId, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _deleteKittenClawsFunction.Delete(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
