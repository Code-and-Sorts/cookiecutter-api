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

public class CreateKittenClawsTest
{
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly ILogger<CreateKittenClaws> _mockLogger;
    private readonly CreateKittenClaws _createKittenClawsFunction;

    public CreateKittenClawsTest()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _mockLogger = Substitute.For<ILogger<CreateKittenClaws>>();
        _createKittenClawsFunction = new CreateKittenClaws(_mockKittenClawsController, _mockLogger);
    }

    [Fact]
    public async Task Post_ReturnsCreatedResult_WhenKittenClawsIsCreated()
    {
        // Arrange
        var createKittenClawsRequest = new CreateKittenClawsRequest { Name = "mockKittenClaws" };
        var newKittenClawsDto = new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws" };
        var request = Mocks.CreateApiGatewayRequest(createKittenClawsRequest, "POST");

        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newKittenClawsDto));

        // Act
        var response = await _createKittenClawsFunction.Post(request);
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(response.Body);

        // Assert
        Assert.Equal(201, response.StatusCode);
        Assert.Equal(newKittenClawsDto.Id, responseDto!.Id);
        Assert.Equal(newKittenClawsDto.Name, responseDto.Name);
    }

    [Fact]
    public async Task Post_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var createKittenClawsRequest = new CreateKittenClawsRequest { Name = "mockKittenClaws" };
        var request = Mocks.CreateApiGatewayRequest(createKittenClawsRequest, "POST");

        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _createKittenClawsFunction.Post(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
