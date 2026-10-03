namespace KittenClaws.Api.Dtos;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class VisitDto : BaseResponse
{
    public VisitDto()
    {
    }

    public VisitDto(VisitEntity item)
        : base(item)
    {
        Reason = item.Reason;
        VisitedOn = item.VisitedOn;
        Cost = item.Cost;
        Paid = item.Paid ?? false;
        CheckedAt = item.CheckedAt;
    }

    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Reason { get; set; }

    [JsonPropertyName("visitedOn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? VisitedOn { get; set; }

    [JsonPropertyName("cost")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public double? Cost { get; set; }

    [JsonPropertyName("paid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? Paid { get; set; }

    [JsonPropertyName("checkedAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<string>? CheckedAt { get; set; }
}
