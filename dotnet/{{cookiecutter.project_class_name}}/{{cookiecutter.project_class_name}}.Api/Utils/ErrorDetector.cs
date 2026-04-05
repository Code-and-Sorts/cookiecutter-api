{%- if cookiecutter.cloud_service in ['Azure Function App', 'GCP Cloud Function'] %}
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
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
namespace {{cookiecutter.project_class_name}}.Api.Utils;

using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

public class BaseError
{
    public required string ErrorMessage { get; set; }
}

public static class ErrorDetector
{
    public static APIGatewayProxyResponse DetectError<T>(T error)
    {
        var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };

        if (error is ResourceNotFoundException notFoundError)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Headers = headers,
                Body = JsonConvert.SerializeObject(new BaseError { ErrorMessage = notFoundError.Message })
            };
        }
        if (error is KeyNotFoundException keyNotFoundError)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Headers = headers,
                Body = JsonConvert.SerializeObject(new BaseError { ErrorMessage = keyNotFoundError.Message })
            };
        }
        if (error is Exception baseError)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = headers,
                Body = JsonConvert.SerializeObject(new BaseError { ErrorMessage = baseError.Message })
            };
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 500,
            Headers = headers,
            Body = JsonConvert.SerializeObject(new BaseError { ErrorMessage = "Unknown error occurred." })
        };
    }
}

public static class ResponseHelper
{
    private static readonly Dictionary<string, string> JsonHeaders = new() { { "Content-Type", "application/json" } };

    public static APIGatewayProxyResponse Ok(object body) => new()
    {
        StatusCode = 200,
        Headers = JsonHeaders,
        Body = JsonConvert.SerializeObject(body)
    };

    public static APIGatewayProxyResponse Created(object body) => new()
    {
        StatusCode = 201,
        Headers = JsonHeaders,
        Body = JsonConvert.SerializeObject(body)
    };
}
{%- endif %}
