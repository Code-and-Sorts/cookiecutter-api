namespace {{cookiecutter.project_name}}.Api.Tests.Unit;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using {{cookiecutter.project_name}}.Api.Functions;
using {{cookiecutter.project_name}}.Api.Interfaces;
using {{cookiecutter.project_name}}.Api.Dtos;
using NSubstitute.ExceptionExtensions;
using {{cookiecutter.project_name}}.Api.Utils;

public class Get{{cookiecutter.project_name}}Test
{
    private readonly I{{cookiecutter.project_name}}Controller _mock{{cookiecutter.project_name}}Controller;
    private readonly ILogger<Get{{cookiecutter.project_name}}> _mockLogger;
    private readonly Get{{cookiecutter.project_name}} _get{{cookiecutter.project_name}}Function;

    public Get{{cookiecutter.project_name}}Test()
    {
        _mock{{cookiecutter.project_name}}Controller = Substitute.For<I{{cookiecutter.project_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Get{{cookiecutter.project_name}}>>();
        _get{{cookiecutter.project_name}}Function = new Get{{cookiecutter.project_name}}(_mock{{cookiecutter.project_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Get_ReturnsGetResult_When{{cookiecutter.project_name}}IsGot()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var get{{cookiecutter.project_name}}Dto = new {{cookiecutter.project_name}}Dto { Id = {{cookiecutter.project_lower_camel_name}}Id, Name = "mock{{cookiecutter.project_name}}" };
        _mock{{cookiecutter.project_name}}Controller.GetAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult(get{{cookiecutter.project_name}}Dto));

        // Act
        var result = await _get{{cookiecutter.project_name}}Function.Get({{cookiecutter.project_lower_camel_name}}Id);

        // Assert
        var getResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, getResult.StatusCode);
        Assert.Equal(get{{cookiecutter.project_name}}Dto, getResult.Value);
    }

    [Fact]
    public async Task Get_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mock{{cookiecutter.project_name}}Controller.GetAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _get{{cookiecutter.project_name}}Function.Get({{cookiecutter.project_lower_camel_name}}Id);

        // Assert
        var objectResult = Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
