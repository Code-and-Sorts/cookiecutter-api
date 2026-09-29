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
using KittenClaws.Api.Requests;
using KittenClaws.Api.Dtos;
using NSubstitute.ExceptionExtensions;
using KittenClaws.Api.Utils;
using System.IO;
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
        var httpRequestData = Mocks.CreateHttpRequestData(updateKittenClawsRequest, "PATCH");

        _mockKittenClawsController.UpdateAsync(kittenClawsId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newKittenClawsDto));

        // Act
        var response = await _updateKittenClawsFunction.Patch(httpRequestData, kittenClawsId);

        var updatedResult = Assert.IsType<OkObjectResult>(response);
        var responseBody = JsonConvert.SerializeObject(updatedResult.Value);
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(responseBody);

        // Assert
        Assert.Equal(200, updatedResult.StatusCode);
        Assert.Equal(newKittenClawsDto.Name, responseDto.Name);
    }

    [Fact]
    public async Task Patch_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateKittenClawsRequest = new UpdateKittenClawsRequest { Name = "mockKittenClaws" };
        var httpRequestData = Mocks.CreateHttpRequestData(updateKittenClawsRequest, "PATCH");

        _mockKittenClawsController.UpdateAsync(kittenClawsId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _updateKittenClawsFunction.Patch(httpRequestData, kittenClawsId);

        var updatedResult = Assert.IsType<HttpResponseInit>(response);
        var errorResult = Assert.IsType<BaseError>(updatedResult.Value);

        // Assert
        Assert.Equal(500, updatedResult.StatusCode);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
