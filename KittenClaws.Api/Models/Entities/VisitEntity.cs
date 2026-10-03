namespace KittenClaws.Api.Entities;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class VisitEntity : BaseEntity
{
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("visitedOn")]
    public string? VisitedOn { get; set; }

    [JsonPropertyName("cost")]
    public double? Cost { get; set; }

    [JsonPropertyName("paid")]
    public bool? Paid { get; set; }

    [JsonPropertyName("checkedAt")]
    public List<string>? CheckedAt { get; set; }
}
