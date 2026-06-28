namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using {{project_class_name}}.Api.Functions;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Utils;
using Newtonsoft.Json;

public class Update{{project_class_name}}Test
{
    private readonly I{{project_class_name}}Controller _mock{{project_class_name}}Controller;
    private readonly ILogger<Update{{project_class_name}}> _mockLogger;
    private readonly Update{{project_class_name}} _update{{project_class_name}}Function;

    public Update{{project_class_name}}Test()
    {
        _mock{{project_class_name}}Controller = Substitute.For<I{{project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Update{{project_class_name}}>>();
        _update{{project_class_name}}Function = new Update{{project_class_name}}(_mock{{project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Patch_ReturnsUpdatedResult_When{{project_class_name}}IsUpdated()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var update{{project_class_name}}Request = new Update{{project_class_name}}Request { Name = "mock{{project_class_name}}" };
        var new{{project_class_name}}Dto = new {{project_class_name}}Dto { Id = {{project_lower_camel_name}}Id, Name = "mock{{project_class_name}}" };
        var request = Mocks.CreateApiGatewayRequest(update{{project_class_name}}Request, "PATCH", new Dictionary<string, string> { { "id", {{project_lower_camel_name}}Id } });

        _mock{{project_class_name}}Controller.UpdateAsync({{project_lower_camel_name}}Id, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new{{project_class_name}}Dto));

        // Act
        var response = await _update{{project_class_name}}Function.Patch(request);
        var responseDto = JsonConvert.DeserializeObject<{{project_class_name}}Dto>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(new{{project_class_name}}Dto.Name, responseDto!.Name);
    }

    [Fact]
    public async Task Patch_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var update{{project_class_name}}Request = new Update{{project_class_name}}Request { Name = "mock{{project_class_name}}" };
        var request = Mocks.CreateApiGatewayRequest(update{{project_class_name}}Request, "PATCH", new Dictionary<string, string> { { "id", {{project_lower_camel_name}}Id } });

        _mock{{project_class_name}}Controller.UpdateAsync({{project_lower_camel_name}}Id, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _update{{project_class_name}}Function.Patch(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
