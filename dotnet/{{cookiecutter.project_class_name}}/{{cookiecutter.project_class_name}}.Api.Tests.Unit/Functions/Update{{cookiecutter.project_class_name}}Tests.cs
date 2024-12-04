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
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Requests;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Dtos;
using NSubstitute.ExceptionExtensions;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Utils;
using System.IO;
using Newtonsoft.Json;

public class Update{{cookiecutter.{{cookiecutter.project_class_name}}}}Test
{
    private readonly I{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller;
    private readonly ILogger<Update{{cookiecutter.{{cookiecutter.project_class_name}}}}> _mockLogger;
    private readonly Update{{cookiecutter.{{cookiecutter.project_class_name}}}} _update{{cookiecutter.{{cookiecutter.project_class_name}}}}Function;

    public Update{{cookiecutter.{{cookiecutter.project_class_name}}}}Test()
    {
        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller = Substitute.For<I{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller>();
        _mockLogger = Substitute.For<ILogger<Update{{cookiecutter.{{cookiecutter.project_class_name}}}}>>();
        _update{{cookiecutter.{{cookiecutter.project_class_name}}}}Function = new Update{{cookiecutter.{{cookiecutter.project_class_name}}}}(_mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Patch_ReturnsUpdatedResult_When{{cookiecutter.{{cookiecutter.project_class_name}}}}IsUpdated()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request = new Update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request { Name = "mock{{cookiecutter.{{cookiecutter.project_class_name}}}}" };
        var new{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto = new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = {{cookiecutter.project_lower_camel_name}}Id, Name = "mock{{cookiecutter.{{cookiecutter.project_class_name}}}}" };
        var httpRequestData = Mocks.CreateHttpRequestData(update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request, "PATCH");

        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller.UpdateAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto));

        // Act
        var response = await _update{{cookiecutter.{{cookiecutter.project_class_name}}}}Function.Patch(httpRequestData, {{cookiecutter.project_lower_camel_name}}Id);

        var updatedResult = Assert.IsType<OkObjectResult>(response);
        var responseBody = JsonConvert.SerializeObject(updatedResult.Value);
        var responseDto = JsonConvert.DeserializeObject<{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto>(responseBody);

        // Assert
        Assert.Equal(200, updatedResult.StatusCode);
        Assert.Equal(new{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto.Name, responseDto.Name);
    }

    [Fact]
    public async Task Patch_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request = new Update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request { Name = "mock{{cookiecutter.{{cookiecutter.project_class_name}}}}" };
        var httpRequestData = Mocks.CreateHttpRequestData(update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request, "PATCH");

        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Controller.UpdateAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _update{{cookiecutter.{{cookiecutter.project_class_name}}}}Function.Patch(httpRequestData, {{cookiecutter.project_lower_camel_name}}Id);

        var updatedResult = Assert.IsType<HttpResponseInit>(response);
        var errorResult = Assert.IsType<BaseError>(updatedResult.Value);

        // Assert
        Assert.Equal(500, updatedResult.StatusCode);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
