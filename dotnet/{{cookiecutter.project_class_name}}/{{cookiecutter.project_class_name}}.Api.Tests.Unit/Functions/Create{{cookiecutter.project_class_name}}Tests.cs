namespace {{cookiecutter.project_class_name}}.Api.Tests.Unit;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using {{cookiecutter.project_class_name}}.Api.Functions;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Requests;
using {{cookiecutter.project_class_name}}.Api.Dtos;
using NSubstitute.ExceptionExtensions;
using {{cookiecutter.project_class_name}}.Api.Utils;

public class Create{{cookiecutter.project_class_name}}Test
{
    private readonly I{{cookiecutter.project_class_name}}Controller _mock{{cookiecutter.project_class_name}}Controller;
    private readonly ILogger<Create{{cookiecutter.project_class_name}}> _mockLogger;
    private readonly Create{{cookiecutter.project_class_name}} _create{{cookiecutter.project_class_name}}Function;

    public Create{{cookiecutter.project_class_name}}Test()
    {
        _mock{{cookiecutter.project_class_name}}Controller = Substitute.For<I{{cookiecutter.project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Create{{cookiecutter.project_class_name}}>>();
        _create{{cookiecutter.project_class_name}}Function = new Create{{cookiecutter.project_class_name}}(_mock{{cookiecutter.project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Post_ReturnsCreatedResult_When{{cookiecutter.project_class_name}}IsCreated()
    {
        // Arrange
        var create{{cookiecutter.project_class_name}}Request = new Create{{cookiecutter.project_class_name}}Request { Name = "mock{{cookiecutter.project_class_name}}" };
        var new{{cookiecutter.project_class_name}}Dto = new {{cookiecutter.project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}" };
        _mock{{cookiecutter.project_class_name}}Controller.CreateAsync(create{{cookiecutter.project_class_name}}Request, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new{{cookiecutter.project_class_name}}Dto));

        // Act
        var result = await _create{{cookiecutter.project_class_name}}Function.Post(create{{cookiecutter.project_class_name}}Request);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal($"/api/{{cookiecutter.project_endpoint}}", createdResult.Location);
        Assert.Equal(new{{cookiecutter.project_class_name}}Dto, createdResult.Value);
    }

    [Fact]
    public async Task Post_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var create{{cookiecutter.project_class_name}}Request = new Create{{cookiecutter.project_class_name}}Request { Name = "mock{{cookiecutter.project_class_name}}" };
        _mock{{cookiecutter.project_class_name}}Controller.CreateAsync(create{{cookiecutter.project_class_name}}Request, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _create{{cookiecutter.project_class_name}}Function.Post(create{{cookiecutter.project_class_name}}Request);

        // Assert
        var objectResult = Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
