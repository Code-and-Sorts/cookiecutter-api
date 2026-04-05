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
        var request = Mocks.CreateApiGatewayRequest("DELETE", new Dictionary<string, string> { { "id", {{cookiecutter.project_lower_camel_name}}Id } });

        _mock{{cookiecutter.project_class_name}}Controller.DeleteAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await _delete{{cookiecutter.project_class_name}}Function.Delete(request);
        var deleteResult = JsonConvert.DeserializeObject<DeleteOkObjectResult>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("{{cookiecutter.project_class_name}} with id 0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c was deleted successfully.", deleteResult!.Message);
    }

    [Fact]
    public async Task Delete_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var request = Mocks.CreateApiGatewayRequest("DELETE", new Dictionary<string, string> { { "id", {{cookiecutter.project_lower_camel_name}}Id } });

        _mock{{cookiecutter.project_class_name}}Controller.DeleteAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _delete{{cookiecutter.project_class_name}}Function.Delete(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
