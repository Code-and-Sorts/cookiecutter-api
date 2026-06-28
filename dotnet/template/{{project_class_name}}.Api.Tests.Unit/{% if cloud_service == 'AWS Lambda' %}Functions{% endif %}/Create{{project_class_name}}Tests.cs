namespace {{project_class_name}}.Api.Tests.Unit;

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
using {{project_class_name}}.Api.Functions;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Utils;
using Newtonsoft.Json;

public class Create{{project_class_name}}Test
{
    private readonly I{{project_class_name}}Controller _mock{{project_class_name}}Controller;
    private readonly ILogger<Create{{project_class_name}}> _mockLogger;
    private readonly Create{{project_class_name}} _create{{project_class_name}}Function;

    public Create{{project_class_name}}Test()
    {
        _mock{{project_class_name}}Controller = Substitute.For<I{{project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Create{{project_class_name}}>>();
        _create{{project_class_name}}Function = new Create{{project_class_name}}(_mock{{project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Post_ReturnsCreatedResult_When{{project_class_name}}IsCreated()
    {
        // Arrange
        var create{{project_class_name}}Request = new Create{{project_class_name}}Request { Name = "mock{{project_class_name}}" };
        var new{{project_class_name}}Dto = new {{project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}" };
        var request = Mocks.CreateApiGatewayRequest(create{{project_class_name}}Request, "POST");

        _mock{{project_class_name}}Controller.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new{{project_class_name}}Dto));

        // Act
        var response = await _create{{project_class_name}}Function.Post(request);
        var responseDto = JsonConvert.DeserializeObject<{{project_class_name}}Dto>(response.Body);

        // Assert
        Assert.Equal(201, response.StatusCode);
        Assert.Equal(new{{project_class_name}}Dto.Id, responseDto!.Id);
        Assert.Equal(new{{project_class_name}}Dto.Name, responseDto.Name);
    }

    [Fact]
    public async Task Post_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var create{{project_class_name}}Request = new Create{{project_class_name}}Request { Name = "mock{{project_class_name}}" };
        var request = Mocks.CreateApiGatewayRequest(create{{project_class_name}}Request, "POST");

        _mock{{project_class_name}}Controller.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _create{{project_class_name}}Function.Post(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
