namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using KittenClaws.Api.Functions;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Utils;
using Newtonsoft.Json;

public class UpdateKittenClawsTest
{
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly ILogger<UpdateKittenClaws> _mockLogger;
    private readonly UpdateKittenClaws _updateKittenClawsFunction;

    public UpdateKittenClawsTest()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _mockLogger = Substitute.For<ILogger<UpdateKittenClaws>>();
        _updateKittenClawsFunction = new UpdateKittenClaws(_mockKittenClawsController, _mockLogger);
    }

    [Fact]
    public async Task Patch_ReturnsUpdatedResult_WhenKittenClawsIsUpdated()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateKittenClawsRequest = new UpdateKittenClawsRequest { Name = "mockKittenClaws" };
        var newKittenClawsDto = new KittenClawsDto { Id = kittenClawsId, Name = "mockKittenClaws" };
        var request = Mocks.CreateApiGatewayRequest(updateKittenClawsRequest, "PATCH", new Dictionary<string, string> { { "id", kittenClawsId } });

        _mockKittenClawsController.UpdateAsync(kittenClawsId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newKittenClawsDto));

        // Act
        var response = await _updateKittenClawsFunction.Patch(request);
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(newKittenClawsDto.Name, responseDto!.Name);
    }

    [Fact]
    public async Task Patch_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateKittenClawsRequest = new UpdateKittenClawsRequest { Name = "mockKittenClaws" };
        var request = Mocks.CreateApiGatewayRequest(updateKittenClawsRequest, "PATCH", new Dictionary<string, string> { { "id", kittenClawsId } });

        _mockKittenClawsController.UpdateAsync(kittenClawsId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _updateKittenClawsFunction.Patch(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
