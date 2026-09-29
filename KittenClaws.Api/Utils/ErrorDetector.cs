
namespace KittenClaws.Api.Utils;

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
