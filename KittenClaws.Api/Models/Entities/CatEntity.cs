namespace KittenClaws.Api.Entities;

using System.Text.Json.Serialization;

public class CatEntity : BaseEntity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
