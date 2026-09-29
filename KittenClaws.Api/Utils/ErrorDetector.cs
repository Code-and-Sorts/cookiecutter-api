
namespace KittenClaws.Api.Utils;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Grpc.Core;

public class BaseError
{
    public required string ErrorMessage { get; set; }
}

public class HttpResponseInit : ObjectResult
{
    public HttpResponseInit(object value, int statusCode = 500)
        : base(value)
    {
        base.StatusCode = statusCode;
    }
}

public static class ErrorDetector
{
    public static HttpResponseInit DetectError<T>(T error)
    {
        if (error is RpcException rpcError)
        {
            var statusCode = rpcError.StatusCode switch
            {
                StatusCode.NotFound => 404,
                StatusCode.AlreadyExists => 409,
                StatusCode.InvalidArgument => 400,
                StatusCode.PermissionDenied => 403,
                StatusCode.Unauthenticated => 401,
                _ => 500,
            };
            return new HttpResponseInit(new BaseError() { ErrorMessage = rpcError.Message }, statusCode);
        }
        if (error is KeyNotFoundException keyNotFoundError)
        {
            return new HttpResponseInit(new BaseError() { ErrorMessage = keyNotFoundError.Message }, 404);
        }
        if (error is Exception baseError)
        {
            return new HttpResponseInit(new BaseError() { ErrorMessage = baseError.Message });
        }

        return new HttpResponseInit(new BaseError() { ErrorMessage = "Unknown error occurred." });
    }
}
