{%- if cloud_service in ['Azure Function App', 'GCP Cloud Function'] %}
namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Net;
using {{project_class_name}}.Api.Utils;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
using Grpc.Core;
{%- endif %}
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
{%- if cloud_service == 'Azure Function App' %}

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
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    [Fact]
    public void DetectError_WithRpcException_ReturnsHttpResponseInitWithErrorMessage()
    {
        // Arrange
        var exception = new RpcException(new Status(StatusCode.NotFound, "Mock Firestore exception"));

        // Act
        var result = ErrorDetector.DetectError(exception);

        // Assert
        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(404, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Contains("Mock Firestore exception", baseError.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithKeyNotFoundException_ReturnsHttpResponseInitWith404()
    {
        // Arrange
        var exception = new KeyNotFoundException("Item not found");

        // Act
        var result = ErrorDetector.DetectError(exception);

        // Assert
        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(404, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Equal("Item not found", baseError.ErrorMessage);
    }
{%- endif %}

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
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using {{project_class_name}}.Api.Utils;
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
{%- endif %}
