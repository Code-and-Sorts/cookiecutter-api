namespace KittenClaws.Api.Utils;

using System.Text.Json.Serialization;

public class DeleteOkObjectResult
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    public static DeleteOkObjectResult For(string resourceName, string id) =>
        new() { Message = $"{resourceName} with id {id} was deleted successfully." };
}
