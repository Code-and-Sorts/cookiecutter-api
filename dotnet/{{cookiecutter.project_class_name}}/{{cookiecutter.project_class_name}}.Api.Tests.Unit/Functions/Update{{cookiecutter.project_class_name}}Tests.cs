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

public class Update{{cookiecutter.project_class_name}}Test
{
    private readonly I{{cookiecutter.project_class_name}}Controller _mock{{cookiecutter.project_class_name}}Controller;
    private readonly ILogger<Update{{cookiecutter.project_class_name}}> _mockLogger;
    private readonly Update{{cookiecutter.project_class_name}} _update{{cookiecutter.project_class_name}}Function;

    public Update{{cookiecutter.project_class_name}}Test()
    {
        _mock{{cookiecutter.project_class_name}}Controller = Substitute.For<I{{cookiecutter.project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Update{{cookiecutter.project_class_name}}>>();
        _update{{cookiecutter.project_class_name}}Function = new Update{{cookiecutter.project_class_name}}(_mock{{cookiecutter.project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Patch_ReturnsUpdatedResult_When{{cookiecutter.project_class_name}}IsUpdated()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var update{{cookiecutter.project_class_name}}Request = new Update{{cookiecutter.project_class_name}}Request { Name = "mock{{cookiecutter.project_class_name}}" };
        var new{{cookiecutter.project_class_name}}Dto = new {{cookiecutter.project_class_name}}Dto { Id = {{cookiecutter.project_lower_camel_name}}Id, Name = "mock{{cookiecutter.project_class_name}}" };
        _mock{{cookiecutter.project_class_name}}Controller.UpdateAsync(update{{cookiecutter.project_class_name}}Request, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new{{cookiecutter.project_class_name}}Dto));

        // Act
        var result = await _update{{cookiecutter.project_class_name}}Function.Patch(update{{cookiecutter.project_class_name}}Request, {{cookiecutter.project_lower_camel_name}}Id);

        // Assert
        var updatedResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, updatedResult.StatusCode);
        Assert.Equal(new{{cookiecutter.project_class_name}}Dto, updatedResult.Value);
    }

    [Fact]
    public async Task Patch_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var update{{cookiecutter.project_class_name}}Request = new Update{{cookiecutter.project_class_name}}Request { Name = "mock{{cookiecutter.project_class_name}}" };
        _mock{{cookiecutter.project_class_name}}Controller.UpdateAsync(update{{cookiecutter.project_class_name}}Request, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _update{{cookiecutter.project_class_name}}Function.Patch(update{{cookiecutter.project_class_name}}Request, {{cookiecutter.project_lower_camel_name}}Id);

        // Assert
        var objectResult = Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
