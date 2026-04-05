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

public class Get{{cookiecutter.project_class_name}}ListTest
{
    private readonly I{{cookiecutter.project_class_name}}Controller _mock{{cookiecutter.project_class_name}}Controller;
    private readonly ILogger<Get{{cookiecutter.project_class_name}}List> _mockLogger;
    private readonly Get{{cookiecutter.project_class_name}}List _get{{cookiecutter.project_class_name}}Function;

    public Get{{cookiecutter.project_class_name}}ListTest()
    {
        _mock{{cookiecutter.project_class_name}}Controller = Substitute.For<I{{cookiecutter.project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Get{{cookiecutter.project_class_name}}List>>();
        _get{{cookiecutter.project_class_name}}Function = new Get{{cookiecutter.project_class_name}}List(_mock{{cookiecutter.project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Get_ReturnsGetListResult_When{{cookiecutter.project_class_name}}ListIsGot()
    {
        // Arrange
        var getList{{cookiecutter.project_class_name}}Dto = new List<{{cookiecutter.project_class_name}}Dto>
        {
            new {{cookiecutter.project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}1" },
            new {{cookiecutter.project_class_name}}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{cookiecutter.project_class_name}}2" },
        };
        var request = Mocks.CreateApiGatewayRequest("GET");

        _mock{{cookiecutter.project_class_name}}Controller.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IEnumerable<{{cookiecutter.project_class_name}}Dto>)getList{{cookiecutter.project_class_name}}Dto));

        // Act
        var response = await _get{{cookiecutter.project_class_name}}Function.Get(request);
        var responseDtos = JsonConvert.DeserializeObject<List<{{cookiecutter.project_class_name}}Dto>>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(2, responseDtos!.Count);
    }

    [Fact]
    public async Task Get_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var request = Mocks.CreateApiGatewayRequest("GET");

        _mock{{cookiecutter.project_class_name}}Controller.GetListAsync(Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _get{{cookiecutter.project_class_name}}Function.Get(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
