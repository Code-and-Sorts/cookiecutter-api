namespace {{project_class_name}}.Api.Utils;
{% if cloud_service == 'AWS Lambda' %}
using System;
{%- endif %}
using System.Linq;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Functions.Worker.Http;
{%- elif cloud_service == 'GCP Cloud Function' %}
using Microsoft.AspNetCore.Http;
{%- else %}
using Amazon.Lambda.APIGatewayEvents;
{%- endif %}

public static class UserIds
{
    public const string Header = "X-User-Id";

    public const int MaxLength = 256;

    public static readonly string TooLongMessage = $"{Header} must be at most {MaxLength} characters.";
{%- if cloud_service == 'Azure Function App' %}

    public static string? From(HttpRequestData request) =>
        Normalize(request.Headers.TryGetValues(Header, out var values) ? values.FirstOrDefault() : null);
{%- elif cloud_service == 'GCP Cloud Function' %}

    public static string? From(HttpRequest request) => Normalize(request.Headers[Header].FirstOrDefault());
{%- else %}

    // API Gateway proxy events keep the client's header casing.
    public static string? From(APIGatewayProxyRequest request) =>
        Normalize(request.Headers?.FirstOrDefault(header => string.Equals(header.Key, Header, StringComparison.OrdinalIgnoreCase)).Value);
{%- endif %}

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
