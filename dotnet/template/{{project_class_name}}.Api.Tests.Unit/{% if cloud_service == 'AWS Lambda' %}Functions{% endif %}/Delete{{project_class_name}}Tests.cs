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

public class Delete{{project_class_name}}Test
{
    private readonly I{{project_class_name}}Controller _mock{{project_class_name}}Controller;
    private readonly ILogger<Delete{{project_class_name}}> _mockLogger;
    private readonly Delete{{project_class_name}} _delete{{project_class_name}}Function;

    public Delete{{project_class_name}}Test()
    {
        _mock{{project_class_name}}Controller = Substitute.For<I{{project_class_name}}Controller>();
        _mockLogger = Substitute.For<ILogger<Delete{{project_class_name}}>>();
        _delete{{project_class_name}}Function = new Delete{{project_class_name}}(_mock{{project_class_name}}Controller, _mockLogger);
    }

    [Fact]
    public async Task Delete_ReturnsDeleteResult_When{{project_class_name}}IsDeleted()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var request = Mocks.CreateApiGatewayRequest("DELETE", new Dictionary<string, string> { { "id", {{project_lower_camel_name}}Id } });

        _mock{{project_class_name}}Controller.DeleteAsync({{project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await _delete{{project_class_name}}Function.Delete(request);
        var deleteResult = JsonConvert.DeserializeObject<DeleteOkObjectResult>(response.Body);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("{{project_class_name}} with id 0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c was deleted successfully.", deleteResult!.Message);
    }

    [Fact]
    public async Task Delete_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var request = Mocks.CreateApiGatewayRequest("DELETE", new Dictionary<string, string> { { "id", {{project_lower_camel_name}}Id } });

        _mock{{project_class_name}}Controller.DeleteAsync({{project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock exception"));

        // Act
        var response = await _delete{{project_class_name}}Function.Delete(request);
        var errorResult = JsonConvert.DeserializeObject<BaseError>(response.Body);

        // Assert
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", errorResult!.ErrorMessage);
    }
}
