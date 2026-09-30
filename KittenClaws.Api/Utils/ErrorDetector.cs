namespace KittenClaws.Api.Utils;

using System;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;

public class BaseError
{
    [JsonPropertyName("errorMessage")]
    public required string ErrorMessage { get; set; }
}

public static class ErrorDetector
{
    // The generic 500 message keeps exception text and SDK diagnostics away from the client.
    public static (int StatusCode, BaseError Body) Classify(Exception error, ILogger logger, CancellationToken requestAborted = default)
    {
        switch (error)
        {
            // SDKs report cancellation their own way (gRPC: RpcException Cancelled), so check the token.
            case not null when requestAborted.IsCancellationRequested:
                logger.LogInformation("The client cancelled the request.");
                return (500, new BaseError { ErrorMessage = ErrorMessages.Unexpected });
            case ApiException apiError:
                return (apiError.StatusCode, new BaseError { ErrorMessage = apiError.Message });
            default:
                logger.LogError(error, "Unexpected error while handling the request.");
                return (500, new BaseError { ErrorMessage = ErrorMessages.Unexpected });
        }
    }
}
