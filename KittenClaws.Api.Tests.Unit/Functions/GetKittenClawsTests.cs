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
        var getKittenClawsDto = new KittenClawsDto { Id = kittenClawsId, Name = "mockKittenClaws" };
        var request = Mocks.CreateApiGatewayRequest("GET", new Dictionary<string, string> { { "id", kittenClawsId } });

        _mockKittenClawsController.GetAsync(kittenClawsId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(getKittenClawsDto));

        // Act
        var response = await _getKittenClawsFunction.Get(request);
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(getKittenClawsDto.Id, responseDto!.Id);
        Assert.Equal(getKittenClawsDto.Name, responseDto.Name);
    }

    [Fact]
    public async Task Get_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var request = Mocks.CreateApiGatewayRequest("GET", new Dictionary<string, string> { { "id", kittenClawsId } });

        _mockKittenClawsController.GetAsync(kittenClawsId, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _getKittenClawsFunction.Get(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
