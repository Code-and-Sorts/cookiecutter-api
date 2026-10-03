namespace KittenClaws.Api.Requests;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Utils;

public class CreateCatRequest : BaseCreateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("breed")]
    public string? Breed { get; set; } = "tabby";

    [JsonPropertyName("ageYears")]
    public long? AgeYears { get; set; } = 0;

    [JsonPropertyName("weightKg")]
    public double? WeightKg { get; set; }

    [JsonPropertyName("indoor")]
    public bool? Indoor { get; set; } = true;

    [JsonPropertyName("birthDate")]
    public string? BirthDate { get; set; } = Fields.Today();

    [JsonPropertyName("microchipId")]
    public string? MicrochipId { get; set; } = Fields.NewUuid();

    [JsonPropertyName("ownerEmail")]
    public string? OwnerEmail { get; set; } = "unknown@example.com";

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("tagCode")]
    public string? TagCode { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; } = [];

    [JsonPropertyName("scores")]
    public List<long>? Scores { get; set; }

    [JsonPropertyName("adoptedAt")]
    public string? AdoptedAt { get; set; } = Fields.Now();

    [JsonPropertyName("lastVisit")]
    public string? LastVisit { get; set; } = "2026-01-01T00:00:00.000Z";

    [JsonPropertyName("notes")]
    public string? Notes { get; set; } = "$none";

    public CatEntity ToEntity()
    {
        var item = new CatEntity();
        ApplyTo(item);
        item.Name = Name;
        item.Breed = Breed;
        item.AgeYears = AgeYears;
        item.WeightKg = WeightKg;
        item.Indoor = Indoor;
        item.BirthDate = BirthDate;
        item.MicrochipId = MicrochipId;
        item.OwnerEmail = OwnerEmail;
        item.Website = Website;
        item.TagCode = TagCode;
        item.Tags = Tags;
        item.Scores = Scores;
        item.AdoptedAt = Fields.UtcDateTime(AdoptedAt);
        item.LastVisit = Fields.UtcDateTime(LastVisit);
        item.Notes = Notes;
        return item;
    }
}

public class ReplaceCatRequest : BaseReplaceRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("breed")]
    public string? Breed { get; set; } = "tabby";

    [JsonPropertyName("ageYears")]
    public long? AgeYears { get; set; } = 0;

    [JsonPropertyName("weightKg")]
    public double? WeightKg { get; set; }

    [JsonPropertyName("indoor")]
    public bool? Indoor { get; set; } = true;

    [JsonPropertyName("birthDate")]
    public string? BirthDate { get; set; } = Fields.Today();

    [JsonPropertyName("ownerEmail")]
    public string? OwnerEmail { get; set; } = "unknown@example.com";

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("tagCode")]
    public string? TagCode { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; } = [];

    [JsonPropertyName("scores")]
    public List<long>? Scores { get; set; }

    [JsonPropertyName("adoptedAt")]
    public string? AdoptedAt { get; set; } = Fields.Now();

    [JsonPropertyName("lastVisit")]
    public string? LastVisit { get; set; } = "2026-01-01T00:00:00.000Z";

    [JsonPropertyName("notes")]
    public string? Notes { get; set; } = "$none";

    public void ApplyTo(CatEntity item)
    {
        base.ApplyTo(item);
        item.Name = Name;
        item.Breed = Breed;
        item.AgeYears = AgeYears;
        item.WeightKg = WeightKg;
        item.Indoor = Indoor;
        item.BirthDate = BirthDate;
        item.OwnerEmail = OwnerEmail;
        item.Website = Website;
        item.TagCode = TagCode;
        item.Tags = Tags;
        item.Scores = Scores;
        item.AdoptedAt = Fields.UtcDateTime(AdoptedAt);
        item.LastVisit = Fields.UtcDateTime(LastVisit);
        item.Notes = Notes;
    }
}

public class UpdateCatRequest : BaseUpdateRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("ageYears")]
    public long? AgeYears { get; set; }

    [JsonPropertyName("weightKg")]
    public double? WeightKg { get; set; }

    [JsonPropertyName("indoor")]
    public bool? Indoor { get; set; }

    [JsonPropertyName("ownerEmail")]
    public string? OwnerEmail { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("adoptedAt")]
    public string? AdoptedAt { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    public void ApplyTo(CatEntity item)
    {
        base.ApplyTo(item);
        if (Sent.Contains("name"))
        {
            item.Name = Name;
        }
        if (Sent.Contains("ageYears"))
        {
            item.AgeYears = AgeYears;
        }
        if (Sent.Contains("weightKg"))
        {
            item.WeightKg = WeightKg;
        }
        if (Sent.Contains("indoor"))
        {
            item.Indoor = Indoor;
        }
        if (Sent.Contains("ownerEmail"))
        {
            item.OwnerEmail = OwnerEmail;
        }
        if (Sent.Contains("website"))
        {
            item.Website = Website;
        }
        if (Sent.Contains("tags"))
        {
            item.Tags = Tags;
        }
        if (Sent.Contains("adoptedAt"))
        {
            item.AdoptedAt = Fields.UtcDateTime(AdoptedAt);
        }
        if (Sent.Contains("notes"))
        {
            item.Notes = Notes;
        }
    }
}
