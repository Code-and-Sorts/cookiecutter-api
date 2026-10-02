namespace KittenClaws.Api.Utils;

using System.Linq;
using Microsoft.Azure.Functions.Worker.Http;

public static class UserIds
{
    public const string Header = "X-User-Id";

    public const int MaxLength = 256;

    public static readonly string TooLongMessage = $"{Header} must be at most {MaxLength} characters.";

    public static string? From(HttpRequestData request) =>
        Normalize(request.Headers.TryGetValues(Header, out var values) ? values.FirstOrDefault() : null);

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
