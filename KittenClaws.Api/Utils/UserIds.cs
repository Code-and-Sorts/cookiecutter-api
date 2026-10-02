namespace KittenClaws.Api.Utils;

using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;

public static class UserIds
{
    public const string Header = "X-User-Id";

    public const int MaxLength = 256;

    public static readonly string TooLongMessage = $"{Header} must be at most {MaxLength} characters.";

    // API Gateway proxy events keep the client's header casing.
    public static string? From(APIGatewayProxyRequest request) =>
        Normalize(request.Headers?.FirstOrDefault(header => string.Equals(header.Key, Header, StringComparison.OrdinalIgnoreCase)).Value);

    private static string? Normalize(string? value)
    {
        var userId = value?.Trim();
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }
        if (userId.Length > MaxLength)
        {
            throw new BadRequestException(TooLongMessage);
        }
        return userId;
    }
}
