
namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Net;
using KittenClaws.Api.Utils;
using Microsoft.Azure.Cosmos;
using Xunit;

public class ErrorDetectorTest
{
    [Fact]
    public void DetectError_WithException_ReturnsHttpResponseInitWithErrorMessage()
    {
        var exception = new Exception("Mock exception");

        var result = ErrorDetector.DetectError(exception);

        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Equal("Mock exception", baseError.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithCosmosException_ReturnsHttpResponseInitWithErrorMessage()
    {
        var exception = new CosmosException(
            "Mock Cosmos DB exception",
            HttpStatusCode.BadRequest,
            0,
            string.Empty,
            0
        );

        var result = ErrorDetector.DetectError(exception);

        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(400, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Equal("Mock Cosmos DB exception", baseError.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithNonException_ReturnsHttpResponseInitWithUnknownErrorMessage()
    {
        var error = "Mock some error";

        var result = ErrorDetector.DetectError(error);

        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(500, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Equal("Unknown error occurred.", baseError.ErrorMessage);
    }
}
