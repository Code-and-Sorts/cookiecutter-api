namespace {{cookiecutter.project_class_name}}.Api.Utils;

using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

public class BaseError
{
    public string ErrorMessage { get; set; }
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
        if (error is CosmosException cosmosError)
        {
            var statusCode = (int)cosmosError.StatusCode;
            return new HttpResponseInit(new BaseError() { ErrorMessage = cosmosError.Message }, statusCode);
        }
        if (error is Exception baseError)
        {
            return new HttpResponseInit(new BaseError() { ErrorMessage = baseError.Message });
        }

        return new HttpResponseInit(new BaseError() { ErrorMessage = "Unknown error occurred." });
    }
}
