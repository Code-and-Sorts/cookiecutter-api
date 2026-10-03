namespace {{project_class_name}}.Api.Tests.Unit;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Utils;

// Holds every type a field can have, so the stores are tested apart from the resources' own fields.
public class TestRecord : BaseEntity
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("count")]
    public long? Count { get; set; }

    [JsonPropertyName("ratio")]
    public double? Ratio { get; set; }

    [JsonPropertyName("flag")]
    public bool? Flag { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("counts")]
    public List<long>? Counts { get; set; }

    [JsonPropertyName("ratios")]
    public List<double>? Ratios { get; set; }

    [JsonPropertyName("flags")]
    public List<bool>? Flags { get; set; }

    [JsonPropertyName("empty")]
    public List<string>? Empty { get; set; }

    public static TestRecord Sample(string id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", string text = "stored") => new()
    {
        Id = id,
        CreatedTimestamp = "2026-01-01T00:00:00.000Z",
        UpdatedTimestamp = "2026-01-01T00:00:00.000Z",
        CreatedBy = "User2",
        Text = text,
        Count = Fields.MaxSafeInteger,
        Ratio = 1.5,
        Flag = false,
        Tags = ["a", "b"],
        Counts = [0, -2],
        Ratios = [0.25, 3],
        Flags = [true, false],
        Empty = [],
    };
}
