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
using {{cookiecutter.project_class_name}}.Api.Dtos;
using NSubstitute.ExceptionExtensions;
using {{cookiecutter.project_class_name}}.Api.Utils;

public class Delete{{cookiecutter.project_class_name}}Test
{
    private readonly I{{cookiecutter.project_class_name}}Controller _mock{{cookiecutter.project_class_name}}Controller;
    private readonly ILogger<Delete{{cookiecutter.project_class_name}}> _mockLogger;
    private readonly Delete{{cookiecutter.project_class_name}} _delete{{cookiecutter.project_class_name}}Function;

    public Delete{{cookiecutter.project_class_name}}Test()
    {
        _mock{{cookiecutter.project_class_name}}Controller = Substitute.For<I{{cookiecutter.project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Delete{{cookiecutter.project_class_name}}>>();
        _delete{{cookiecutter.project_class_name}}Function = new Delete{{cookiecutter.project_class_name}}(_mock{{cookiecutter.project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Delete_ReturnsDeleteResult_When{{cookiecutter.project_class_name}}IsDeleted()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var delete{{cookiecutter.project_class_name}}Dto = new {{cookiecutter.project_class_name}}Dto { Id = {{cookiecutter.project_lower_camel_name}}Id, Name = "mock{{cookiecutter.project_class_name}}" };
        _mock{{cookiecutter.project_class_name}}Controller.DeleteAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult(delete{{cookiecutter.project_class_name}}Dto));

        // Act
        var result = await _delete{{cookiecutter.project_class_name}}Function.Delete({{cookiecutter.project_lower_camel_name}}Id);

        // Assert
        var deleteResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, deleteResult.StatusCode);
        var errorResult = Assert.IsType<DeleteOkObjectResult>(deleteResult.Value);
        Assert.Equal("{{cookiecutter.project_class_name}} with id 0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c was deleted successfully.", errorResult.Message);
    }

    [Fact]
    public async Task Delete_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mock{{cookiecutter.project_class_name}}Controller.DeleteAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _delete{{cookiecutter.project_class_name}}Function.Delete({{cookiecutter.project_lower_camel_name}}Id);

        // Assert
        var objectResult = Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
