namespace KittenClaws.Api.Utils;

using System.Linq;
using Microsoft.AspNetCore.Http;

public static class UserIds
{
    public const string Header = "X-User-Id";

    public const int MaxLength = 256;

    public static readonly string TooLongMessage = $"{Header} must be at most {MaxLength} characters.";

    public static string? From(HttpRequest request) => Normalize(request.Headers[Header].FirstOrDefault());

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
