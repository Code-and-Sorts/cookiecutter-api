
namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Net;
using KittenClaws.Api.Utils;
using Grpc.Core;
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
    public void DetectError_WithRpcException_ReturnsHttpResponseInitWithErrorMessage()
    {
        var exception = new RpcException(new Status(StatusCode.NotFound, "Mock Firestore exception"));

        var result = ErrorDetector.DetectError(exception);

        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(404, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Contains("Mock Firestore exception", baseError.ErrorMessage);
    }

    [Fact]
    public void DetectError_WithKeyNotFoundException_ReturnsHttpResponseInitWith404()
    {
        var exception = new KeyNotFoundException("Item not found");

        var result = ErrorDetector.DetectError(exception);

        Assert.IsType<HttpResponseInit>(result);
        Assert.Equal(404, result.StatusCode);
        var baseError = Assert.IsType<BaseError>(result.Value);
        Assert.Equal("Item not found", baseError.ErrorMessage);
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
