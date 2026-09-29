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

public class GetKittenClawsTest
{
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly ILogger<GetKittenClaws> _mockLogger;
    private readonly GetKittenClaws _getKittenClawsFunction;

    public GetKittenClawsTest()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _mockLogger = Substitute.For<ILogger<GetKittenClaws>>();
        _getKittenClawsFunction = new GetKittenClaws(_mockKittenClawsController, _mockLogger);
    }

    [Fact]
    public async Task Get_ReturnsGetResult_WhenKittenClawsIsGot()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var getKittenClawsDto = new KittenClawsDto { Name = "mockKittenClaws" };
        var httpRequestData = Mocks.CreateHttpRequestData(getKittenClawsDto, "GET");

        _mockKittenClawsController.GetAsync(kittenClawsId, Arg.Any<CancellationToken>()).Returns(Task.FromResult(getKittenClawsDto));

        // Act
        var result = await _getKittenClawsFunction.Get(httpRequestData, kittenClawsId);

        var getResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.Equal(200, getResult.StatusCode);
        Assert.Equal(getKittenClawsDto, getResult.Value);
    }

    [Fact]
    public async Task Get_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var getKittenClawsDto = new KittenClawsDto { Name = "mockKittenClaws" };
        var httpRequestData = Mocks.CreateHttpRequestData(getKittenClawsDto, "GET");

        _mockKittenClawsController.GetAsync(kittenClawsId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _getKittenClawsFunction.Get(httpRequestData, kittenClawsId);

        var objectResult = Assert.IsType<HttpResponseInit>(result);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);

        // Assert
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
