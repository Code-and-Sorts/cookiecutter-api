namespace {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Tests.Unit;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Functions;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Interfaces;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Dtos;
using NSubstitute.ExceptionExtensions;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Utils;

public class Delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Test
{
    private readonly I{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller;
    private readonly ILogger<Delete{{cookiecutter.{{cookiecutter.project_class_name}}}}> _mockLogger;
    private readonly Delete{{cookiecutter.{{cookiecutter.project_class_name}}}} _delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Function;

    public Delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Test()
    {
        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller = Substitute.For<I{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller>();
        _mockLogger = Substitute.For<ILogger<Delete{{cookiecutter.{{cookiecutter.project_class_name}}}}>>();
        _delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Function = new Delete{{cookiecutter.{{cookiecutter.project_class_name}}}}(_mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Delete_ReturnsDeleteResult_When{{cookiecutter.{{cookiecutter.project_class_name}}}}IsDeleted()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto = new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = {{cookiecutter.project_lower_camel_name}}Id, Name = "mock{{cookiecutter.{{cookiecutter.project_class_name}}}}" };
        var httpRequestData = Mocks.CreateHttpRequestData(delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto, "DELETE");

        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller.DeleteAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult(delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto));

        // Act
        var result = await _delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Function.Delete(httpRequestData, {{cookiecutter.project_lower_camel_name}}Id);

        var deleteResult = Assert.IsType<OkObjectResult>(result);
        var errorResult = Assert.IsType<DeleteOkObjectResult>(deleteResult.Value);

        // Assert
        Assert.Equal(200, deleteResult.StatusCode);
        Assert.Equal("{{cookiecutter.{{cookiecutter.project_class_name}}}} with id 0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c was deleted successfully.", errorResult.Message);
    }

    [Fact]
    public async Task Delete_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto = new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = {{cookiecutter.project_lower_camel_name}}Id, Name = "mock{{cookiecutter.{{cookiecutter.project_class_name}}}}" };
        var httpRequestData = Mocks.CreateHttpRequestData(delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto, "DELETE");

        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller.DeleteAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _delete{{cookiecutter.{{cookiecutter.project_class_name}}}}Function.Delete(httpRequestData, {{cookiecutter.project_lower_camel_name}}Id);

        var objectResult = Assert.IsType<HttpResponseInit>(result);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);

        // Assert
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
