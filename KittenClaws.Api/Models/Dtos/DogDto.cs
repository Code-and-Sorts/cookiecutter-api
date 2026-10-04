namespace KittenClaws.Api.Dtos;

using System.Text.Json.Serialization;

public class DogDto : BaseDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}
