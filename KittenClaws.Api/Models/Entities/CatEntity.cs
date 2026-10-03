namespace KittenClaws.Api.Entities;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class CatEntity : BaseEntity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("breed")]
    public string? Breed { get; set; }

    [JsonPropertyName("ageYears")]
    public long? AgeYears { get; set; }

    [JsonPropertyName("weightKg")]
    public double? WeightKg { get; set; }

    [JsonPropertyName("indoor")]
    public bool? Indoor { get; set; }

    [JsonPropertyName("birthDate")]
    public string? BirthDate { get; set; }

    [JsonPropertyName("microchipId")]
    public string? MicrochipId { get; set; }

    [JsonPropertyName("ownerEmail")]
    public string? OwnerEmail { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("tagCode")]
    public string? TagCode { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("scores")]
    public List<long>? Scores { get; set; }

    [JsonPropertyName("adoptedAt")]
    public string? AdoptedAt { get; set; }

    [JsonPropertyName("lastVisit")]
    public string? LastVisit { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
