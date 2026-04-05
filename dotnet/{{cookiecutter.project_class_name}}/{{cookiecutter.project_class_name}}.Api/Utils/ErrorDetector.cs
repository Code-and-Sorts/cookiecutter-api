namespace {{cookiecutter.project_class_name}}.Api.Utils;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
using Grpc.Core;
{%- endif %}

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
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        if (error is CosmosException cosmosError)
        {
            var statusCode = (int)cosmosError.StatusCode;
            return new HttpResponseInit(new BaseError() { ErrorMessage = cosmosError.Message }, statusCode);
        }
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
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
{%- endif %}
        if (error is Exception baseError)
        {
            return new HttpResponseInit(new BaseError() { ErrorMessage = baseError.Message });
        }

        return new HttpResponseInit(new BaseError() { ErrorMessage = "Unknown error occurred." });
    }
}
