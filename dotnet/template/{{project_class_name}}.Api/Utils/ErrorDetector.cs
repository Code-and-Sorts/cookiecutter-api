namespace {{project_class_name}}.Api.Utils;

using System;
{%- if cloud_service == 'AWS Lambda' %}
using System.Collections.Generic;
{%- endif %}
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
{%- if cloud_service == 'AWS Lambda' %}
using Amazon.Lambda.APIGatewayEvents;
{%- else %}
using Microsoft.AspNetCore.Mvc;
{%- endif %}
using Microsoft.Extensions.Logging;
using FluentValidation;

public class BaseError
{
    [JsonPropertyName("errorMessage")]
    public required string ErrorMessage { get; set; }
}
{%- if cloud_service != 'AWS Lambda' %}

public class HttpResponseInit : ObjectResult
{
    public HttpResponseInit(object value, int statusCode = 500)
        : base(value)
    {
        base.StatusCode = statusCode;
    }
}
{%- endif %}

public static class ErrorDetector
{
    public const string UnexpectedErrorMessage = "An unexpected error occurred.";

    /// <summary>
    /// Maps an exception to its status code and <see cref="BaseError"/> body. Expected
    /// client errors (invalid request, item not found) keep their message and are not
    /// logged; anything else is logged with its stack trace and answered with a generic
    /// 500 so that no exception text or SDK diagnostics reach the client.
    /// A request the client cancelled (<paramref name="requestAborted"/>) is not an
    /// unexpected error and is only logged at information level; a passed
    /// <see cref="RequestDeadline"/> is an unexpected error.
    /// </summary>
    public static (int StatusCode, BaseError Body) Classify(Exception error, ILogger logger, CancellationToken requestAborted = default)
    {
        switch (error)
        {
            // SDKs report a cancelled call in their own way (gRPC as RpcException Cancelled),
            // so any failure after the client went away counts as a cancelled request.
            case not null when requestAborted.IsCancellationRequested:
                logger.LogInformation("The client cancelled the request.");
                return (500, new BaseError { ErrorMessage = UnexpectedErrorMessage });
            case ApiException apiError:
                return (apiError.StatusCode, new BaseError { ErrorMessage = apiError.Message });
            case ValidationException validationError:
                var messages = validationError.Errors.Select(failure => failure.ErrorMessage).Distinct();
                return (400, new BaseError { ErrorMessage = string.Join(" ", messages) });
            default:
                logger.LogError(error, "Unexpected error while handling the request.");
                return (500, new BaseError { ErrorMessage = UnexpectedErrorMessage });
        }
    }
{%- if cloud_service == 'AWS Lambda' %}

    public static APIGatewayProxyResponse DetectError(Exception error, ILogger logger, CancellationToken requestAborted = default)
    {
        var (statusCode, body) = Classify(error, logger, requestAborted);
        return ResponseHelper.WithStatus(statusCode, body);
    }
}

public static class ResponseHelper
{
    public static APIGatewayProxyResponse Ok(object body) => WithStatus(200, body);

    public static APIGatewayProxyResponse Created(object body) => WithStatus(201, body);

    public static APIGatewayProxyResponse WithStatus(int statusCode, object body) => new()
    {
        StatusCode = statusCode,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
        Body = Json.Serialize(body),
    };
}
{%- else %}

    public static HttpResponseInit DetectError(Exception error, ILogger logger, CancellationToken requestAborted = default)
    {
        var (statusCode, body) = Classify(error, logger, requestAborted);
        return new HttpResponseInit(body, statusCode);
    }
}
{%- endif %}
