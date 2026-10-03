namespace KittenClaws.Api.Entities;

using System.Text.Json.Serialization;

public class DogEntity : BaseEntity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
