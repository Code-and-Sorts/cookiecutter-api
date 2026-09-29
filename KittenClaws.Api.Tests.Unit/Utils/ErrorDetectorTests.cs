
namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using KittenClaws.Api.Utils;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using Xunit;

public class ErrorDetectorTest
{
    [Fact]
    public void DetectError_WithException_ReturnsApiGatewayResponseWithErrorMessage()
    {
        // Arrange
        var exception = new Exception("Mock exception");

        // Act
        var result = ErrorDetector.DetectError(exception);

        // Assert
        Assert.IsType<APIGatewayProxyResponse>(result);
        Assert.Equal(500, result.StatusCode);
        var baseError = JsonConvert.DeserializeObject<BaseError>(result.Body);
        Assert.Equal("Mock exception", baseError!.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithKeyNotFoundException_ReturnsNotFoundResponse()
    {
        // Arrange
        var exception = new KeyNotFoundException("Item not found");

        // Act
        var result = ErrorDetector.DetectError(exception);

        // Assert
        Assert.IsType<APIGatewayProxyResponse>(result);
        Assert.Equal(404, result.StatusCode);
        var baseError = JsonConvert.DeserializeObject<BaseError>(result.Body);
        Assert.Equal("Item not found", baseError!.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithResourceNotFoundException_ReturnsNotFoundResponse()
    {
        // Arrange
        var exception = new ResourceNotFoundException("DynamoDB resource not found");

        // Act
        var result = ErrorDetector.DetectError(exception);

        // Assert
        Assert.IsType<APIGatewayProxyResponse>(result);
        Assert.Equal(404, result.StatusCode);
        var baseError = JsonConvert.DeserializeObject<BaseError>(result.Body);
        Assert.Equal("DynamoDB resource not found", baseError!.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithNonException_ReturnsApiGatewayResponseWithUnknownErrorMessage()
    {
        // Arrange
        var error = "Mock some error";

        // Act
        var result = ErrorDetector.DetectError(error);

        // Assert
        Assert.IsType<APIGatewayProxyResponse>(result);
        Assert.Equal(500, result.StatusCode);
        var baseError = JsonConvert.DeserializeObject<BaseError>(result.Body);
        Assert.Equal("Unknown error occurred.", baseError!.ErrorMessage);
    }
}
