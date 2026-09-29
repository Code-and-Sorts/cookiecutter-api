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
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Azure.Functions.Worker;

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
        var httpRequestData = Mocks.CreateHttpRequestData(createKittenClawsRequest, "POST");

        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newKittenClawsDto));

        // Act
        var response = await _createKittenClawsFunction.Post(httpRequestData);

        var createdResult = Assert.IsType<CreatedResult>(response);
        var responseBody = JsonConvert.SerializeObject(createdResult.Value);
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(responseBody);

        // Assert
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(newKittenClawsDto.Id, responseDto.Id);
        Assert.Equal(newKittenClawsDto.Name, responseDto.Name);
    }


    [Fact]
    public async Task Post_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var createKittenClawsRequest = new CreateKittenClawsRequest { Name = "mockKittenClaws" };
        var httpRequestData = Mocks.CreateHttpRequestData(createKittenClawsRequest, "POST");

        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _createKittenClawsFunction.Post(httpRequestData);

        var createdResult = Assert.IsType<HttpResponseInit>(response);
        var errorResult = Assert.IsType<BaseError>(createdResult.Value);

        // Assert
        Assert.Equal(500, createdResult.StatusCode);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
