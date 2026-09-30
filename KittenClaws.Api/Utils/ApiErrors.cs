namespace KittenClaws.Api.Utils;

using System;

public static class ErrorMessages
{
    public const string NotFound = "Not found.";

    public const string MethodNotAllowed = "Method not allowed.";

    public const string Unexpected = "An unexpected error occurred.";
}

public abstract class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class BadRequestException(string message) : ApiException(400, message);

public class NotFoundException(string resourceName, string id)
    : ApiException(404, $"{resourceName} with id {id} was not found.");

public static class ItemIds
{
    public static string New() => Guid.NewGuid().ToString();

    // Ids are server-generated UUIDs, so any other value is a 404 like a missing item.
    public static void EnsureValid(string resourceName, string id)
    {
        if (!Guid.TryParseExact(id, "D", out _))
        {
            throw new NotFoundException(resourceName, id);
        }
    }
}
