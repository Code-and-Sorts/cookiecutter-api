namespace KittenClaws.Api.Dtos;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class CatDto : BaseResponse
{
    public CatDto()
    {
    }

    public CatDto(CatEntity item)
        : base(item)
    {
        Name = item.Name;
        Breed = item.Breed ?? "tabby";
        AgeYears = item.AgeYears ?? 0;
        WeightKg = item.WeightKg;
        Indoor = item.Indoor ?? true;
        BirthDate = item.BirthDate;
        MicrochipId = item.MicrochipId;
        OwnerEmail = item.OwnerEmail ?? "unknown@example.com";
        Website = item.Website;
        TagCode = item.TagCode;
        Tags = item.Tags ?? [];
        Scores = item.Scores;
        AdoptedAt = item.AdoptedAt;
        LastVisit = item.LastVisit ?? "2026-01-01T00:00:00.000Z";
    }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Name { get; set; }

    [JsonPropertyName("breed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Breed { get; set; }

    [JsonPropertyName("ageYears")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public long? AgeYears { get; set; }

    [JsonPropertyName("weightKg")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public double? WeightKg { get; set; }

    [JsonPropertyName("indoor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Indoor { get; set; }

    [JsonPropertyName("birthDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? BirthDate { get; set; }

    [JsonPropertyName("microchipId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? MicrochipId { get; set; }

    [JsonPropertyName("ownerEmail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? OwnerEmail { get; set; }

    [JsonPropertyName("website")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Website { get; set; }

    [JsonPropertyName("tagCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? TagCode { get; set; }

    [JsonPropertyName("tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("scores")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<long>? Scores { get; set; }

    [JsonPropertyName("adoptedAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? AdoptedAt { get; set; }

    [JsonPropertyName("lastVisit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? LastVisit { get; set; }
}
