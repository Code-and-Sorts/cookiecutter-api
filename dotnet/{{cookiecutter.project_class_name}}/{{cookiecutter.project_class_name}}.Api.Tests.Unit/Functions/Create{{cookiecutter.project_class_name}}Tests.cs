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
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Azure.Functions.Worker;

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
        var httpRequestData = Mocks.CreateHttpRequestData(create{{cookiecutter.project_class_name}}Request, "POST");

        _mock{{cookiecutter.project_class_name}}Controller.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new{{cookiecutter.project_class_name}}Dto));

        // Act
        var response = await _create{{cookiecutter.project_class_name}}Function.Post(httpRequestData);

        var createdResult = Assert.IsType<CreatedResult>(response);
        var responseBody = JsonConvert.SerializeObject(createdResult.Value);
        var responseDto = JsonConvert.DeserializeObject<{{cookiecutter.project_class_name}}Dto>(responseBody);

        // Assert
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(new{{cookiecutter.project_class_name}}Dto.Id, responseDto.Id);
        Assert.Equal(new{{cookiecutter.project_class_name}}Dto.Name, responseDto.Name);
    }


    [Fact]
    public async Task Post_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var create{{cookiecutter.project_class_name}}Request = new Create{{cookiecutter.project_class_name}}Request { Name = "mock{{cookiecutter.project_class_name}}" };
        var httpRequestData = Mocks.CreateHttpRequestData(create{{cookiecutter.project_class_name}}Request, "POST");

        _mock{{cookiecutter.project_class_name}}Controller.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _create{{cookiecutter.project_class_name}}Function.Post(httpRequestData);

        var createdResult = Assert.IsType<HttpResponseInit>(response);
        var errorResult = Assert.IsType<BaseError>(createdResult.Value);

        // Assert
        Assert.Equal(500, createdResult.StatusCode);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
