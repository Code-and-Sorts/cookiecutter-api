namespace KittenClaws.Api.Entities;

using System.Text.Json.Serialization;

public class KittenClawsEntity : BaseEntity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
