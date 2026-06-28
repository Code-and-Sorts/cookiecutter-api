namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using {{project_class_name}}.Api.Functions;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Utils;
using Newtonsoft.Json;

public class Get{{project_class_name}}Test
{
    private readonly I{{project_class_name}}Controller _mock{{project_class_name}}Controller;
    private readonly ILogger<Get{{project_class_name}}> _mockLogger;
    private readonly Get{{project_class_name}} _get{{project_class_name}}Function;

    public Get{{project_class_name}}Test()
    {
        _mock{{project_class_name}}Controller = Substitute.For<I{{project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Get{{project_class_name}}>>();
        _get{{project_class_name}}Function = new Get{{project_class_name}}(_mock{{project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Get_ReturnsGetResult_When{{project_class_name}}IsGot()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var get{{project_class_name}}Dto = new {{project_class_name}}Dto { Id = {{project_lower_camel_name}}Id, Name = "mock{{project_class_name}}" };
        var request = Mocks.CreateApiGatewayRequest("GET", new Dictionary<string, string> { { "id", {{project_lower_camel_name}}Id } });

        _mock{{project_class_name}}Controller.GetAsync({{project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(get{{project_class_name}}Dto));

        // Act
        var response = await _get{{project_class_name}}Function.Get(request);
        var responseDto = JsonConvert.DeserializeObject<{{project_class_name}}Dto>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(get{{project_class_name}}Dto.Id, responseDto!.Id);
        Assert.Equal(get{{project_class_name}}Dto.Name, responseDto.Name);
    }

    [Fact]
    public async Task Get_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var request = Mocks.CreateApiGatewayRequest("GET", new Dictionary<string, string> { { "id", {{project_lower_camel_name}}Id } });

        _mock{{project_class_name}}Controller.GetAsync({{project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _get{{project_class_name}}Function.Get(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
