namespace {{project_class_name}}.Api.Utils;

using System;

/// <summary>
/// An expected client error. <see cref="ErrorDetector"/> answers it with its
/// status code and message, and does not log it as an error.
/// </summary>
public abstract class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

/// <summary>The request body or a request field is invalid (400).</summary>
public class BadRequestException(string message) : ApiException(400, message);

/// <summary>The item does not exist, is soft-deleted, or its id is not valid (404).</summary>
public class NotFoundException(string resourceName, string id)
    : ApiException(404, $"{resourceName} with id {id} was not found.");

public static class ItemIds
{
    /// <summary>
    /// Ids are server-generated UUIDs, so any other value can never match an item:
    /// answer it with the same 404 as a missing item.
    /// </summary>
    public static void EnsureValid(string resourceName, string id)
    {
        if (!Guid.TryParseExact(id, "D", out _))
        {
            throw new NotFoundException(resourceName, id);
        }
    }
}
