namespace KittenClaws.Api.Requests;

using System.Text.Json.Serialization;

// RequestBody rejects fields without [JsonPropertyName], so clients can never set ids or system fields.

public class CreateDogRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}

public class ReplaceDogRequest
{
    [JsonIgnore]
    public string Id { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}
