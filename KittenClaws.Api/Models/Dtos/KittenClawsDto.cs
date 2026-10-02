namespace KittenClaws.Api.Dtos;

using System.Text.Json.Serialization;

public class KittenClawsDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}
