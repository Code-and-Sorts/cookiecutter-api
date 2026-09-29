namespace {{project_class_name}}.Api.Utils;

using System.Text.Json.Serialization;

public class DeleteOkObjectResult
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}
