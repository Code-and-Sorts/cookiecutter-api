namespace {{cookiecutter.project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using {{cookiecutter.project_class_name}}.Api;
using {{cookiecutter.project_class_name}}.Api.Dtos;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Requests;
using Newtonsoft.Json;

public class FunctionTest
{
    private readonly I{{cookiecutter.project_class_name}}Controller _mock{{cookiecutter.project_class_name}}Controller;
    private readonly ILogger<Function> _mockLogger;
    private readonly Function _function;

    public FunctionTest()
    {
        _mock{{cookiecutter.project_class_name}}Controller = Substitute.For<I{{cookiecutter.project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Function>>();
        _function = new Function(_mock{{cookiecutter.project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task HandleAsync_Get_ReturnsResult()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var {{cookiecutter.project_lower_camel_name}}Dto = new {{cookiecutter.project_class_name}}Dto { Id = id, Name = "mock{{cookiecutter.project_class_name}}" };
        var httpContext = Mocks.CreateHttpContext("GET", "/{{cookiecutter.project_endpoint}}/" + id);

        _mock{{cookiecutter.project_class_name}}Controller.GetAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult({{cookiecutter.project_lower_camel_name}}Dto));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_GetList_ReturnsResults()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}List = new List<{{cookiecutter.project_class_name}}Dto>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{cookiecutter.project_class_name}}2" }
        };
        var httpContext = Mocks.CreateHttpContext("GET", "/{{cookiecutter.project_endpoint}}");

        _mock{{cookiecutter.project_class_name}}Controller.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<{{cookiecutter.project_class_name}}Dto>>({{cookiecutter.project_lower_camel_name}}List));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_Post_ReturnsCreatedResult()
    {
        // Arrange
        var createRequest = new Create{{cookiecutter.project_class_name}}Request { Name = "mock{{cookiecutter.project_class_name}}" };
        var new{{cookiecutter.project_class_name}}Dto = new {{cookiecutter.project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}" };
        var httpContext = Mocks.CreateHttpContext(createRequest, "POST", "/{{cookiecutter.project_endpoint}}");

        _mock{{cookiecutter.project_class_name}}Controller.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new{{cookiecutter.project_class_name}}Dto));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(201, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_Patch_ReturnsUpdatedResult()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new Update{{cookiecutter.project_class_name}}Request { Name = "mockUpdated{{cookiecutter.project_class_name}}" };
        var updated{{cookiecutter.project_class_name}}Dto = new {{cookiecutter.project_class_name}}Dto { Id = id, Name = "mockUpdated{{cookiecutter.project_class_name}}" };
        var httpContext = Mocks.CreateHttpContext(updateRequest, "PATCH", "/{{cookiecutter.project_endpoint}}/" + id);

        _mock{{cookiecutter.project_class_name}}Controller.UpdateAsync(id, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(updated{{cookiecutter.project_class_name}}Dto));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_Delete_ReturnsSuccessResult()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var httpContext = Mocks.CreateHttpContext("DELETE", "/{{cookiecutter.project_endpoint}}/" + id);

        _mock{{cookiecutter.project_class_name}}Controller.DeleteAsync(id, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var httpContext = Mocks.CreateHttpContext("GET", "/{{cookiecutter.project_endpoint}}/" + id);

        _mock{{cookiecutter.project_class_name}}Controller.GetAsync(id, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        await _function.HandleAsync(httpContext);

        // Assert
        Assert.Equal(500, httpContext.Response.StatusCode);
    }
}
