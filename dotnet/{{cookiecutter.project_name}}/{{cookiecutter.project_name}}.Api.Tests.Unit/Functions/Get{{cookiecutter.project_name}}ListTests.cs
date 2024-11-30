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
using System.Collections.Generic;

public class Get{{cookiecutter.project_name}}ListTest
{
    private readonly I{{cookiecutter.project_name}}Controller _mock{{cookiecutter.project_name}}Controller;
    private readonly ILogger<Get{{cookiecutter.project_name}}List> _mockLogger;
    private readonly Get{{cookiecutter.project_name}}List _get{{cookiecutter.project_name}}Function;

    public Get{{cookiecutter.project_name}}ListTest()
    {
        _mock{{cookiecutter.project_name}}Controller = Substitute.For<I{{cookiecutter.project_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Get{{cookiecutter.project_name}}List>>();
        _get{{cookiecutter.project_name}}Function = new Get{{cookiecutter.project_name}}List(_mock{{cookiecutter.project_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Get_ReturnsGetListResult_When{{cookiecutter.project_name}}ListIsGot()
    {
        // Arrange
        var getList{{cookiecutter.project_name}}Dto = new List<{{cookiecutter.project_name}}Dto>
        {
            new {{cookiecutter.project_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_name}}1" },
            new {{cookiecutter.project_name}}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{cookiecutter.project_name}}2" },
        };
        _mock{{cookiecutter.project_name}}Controller.GetListAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult((IEnumerable<{{cookiecutter.project_name}}Dto>)getList{{cookiecutter.project_name}}Dto));

        // Act
        var result = await _get{{cookiecutter.project_name}}Function.Get();

        // Assert
        var getResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, getResult.StatusCode);
        Assert.Equal(getList{{cookiecutter.project_name}}Dto, getResult.Value);
    }

    [Fact]
    public async Task Get_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        _mock{{cookiecutter.project_name}}Controller.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _get{{cookiecutter.project_name}}Function.Get();

        // Assert
        var objectResult = Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
