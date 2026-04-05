namespace {{cookiecutter.project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using {{cookiecutter.project_class_name}}.Api.Functions;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Dtos;
using {{cookiecutter.project_class_name}}.Api.Utils;
using Newtonsoft.Json;

public class Get{{cookiecutter.project_class_name}}Test
{
    private readonly I{{cookiecutter.project_class_name}}Controller _mock{{cookiecutter.project_class_name}}Controller;
    private readonly ILogger<Get{{cookiecutter.project_class_name}}> _mockLogger;
    private readonly Get{{cookiecutter.project_class_name}} _get{{cookiecutter.project_class_name}}Function;

    public Get{{cookiecutter.project_class_name}}Test()
    {
        _mock{{cookiecutter.project_class_name}}Controller = Substitute.For<I{{cookiecutter.project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Get{{cookiecutter.project_class_name}}>>();
        _get{{cookiecutter.project_class_name}}Function = new Get{{cookiecutter.project_class_name}}(_mock{{cookiecutter.project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Get_ReturnsGetResult_When{{cookiecutter.project_class_name}}IsGot()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var get{{cookiecutter.project_class_name}}Dto = new {{cookiecutter.project_class_name}}Dto { Id = {{cookiecutter.project_lower_camel_name}}Id, Name = "mock{{cookiecutter.project_class_name}}" };
        var request = Mocks.CreateApiGatewayRequest("GET", new Dictionary<string, string> { { "id", {{cookiecutter.project_lower_camel_name}}Id } });

        _mock{{cookiecutter.project_class_name}}Controller.GetAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(get{{cookiecutter.project_class_name}}Dto));

        // Act
        var response = await _get{{cookiecutter.project_class_name}}Function.Get(request);
        var responseDto = JsonConvert.DeserializeObject<{{cookiecutter.project_class_name}}Dto>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(get{{cookiecutter.project_class_name}}Dto.Id, responseDto!.Id);
        Assert.Equal(get{{cookiecutter.project_class_name}}Dto.Name, responseDto.Name);
    }

    [Fact]
    public async Task Get_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var request = Mocks.CreateApiGatewayRequest("GET", new Dictionary<string, string> { { "id", {{cookiecutter.project_lower_camel_name}}Id } });

        _mock{{cookiecutter.project_class_name}}Controller.GetAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _get{{cookiecutter.project_class_name}}Function.Get(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
