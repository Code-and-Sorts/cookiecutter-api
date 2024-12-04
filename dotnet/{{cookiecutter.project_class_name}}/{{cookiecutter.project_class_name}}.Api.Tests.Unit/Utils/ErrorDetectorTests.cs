namespace {{cookiecutter.project_class_name}}.Api.Tests.Unit;

using System;
using System.Net;
using {{cookiecutter.project_class_name}}.Api.Utils;
using Microsoft.Azure.Cosmos;
using Xunit;

public class ErrorDetectorTest
{
    [Fact]
    public void DetectError_WithException_ReturnsHttpResponseInitWithErrorMessage()
    {
        // Arrange
        var exception = new Exception("Mock exception");

        // Act
        var result = ErrorDetector.DetectError(exception);

        // Assert
        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Equal("Mock exception", baseError.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithCosmosException_ReturnsHttpResponseInitWithErrorMessage()
    {
        // Arrange
        var exception = new CosmosException(
            "Mock Cosmos DB exception",
            HttpStatusCode.BadRequest,
            0,
            string.Empty,
            0
        );

        // Act
        var result = ErrorDetector.DetectError(exception);

        // Assert
        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(400, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Equal("Mock Cosmos DB exception", baseError.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithNonException_ReturnsHttpResponseInitWithUnknownErrorMessage()
    {
        // Arrange
        var error = "Mock some error";

        // Act
        var result = ErrorDetector.DetectError(error);

        // Assert
        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Equal("Unknown error occurred.", baseError.ErrorMessage);
    }
}
