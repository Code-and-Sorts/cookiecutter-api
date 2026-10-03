namespace KittenClaws.Api.Requests;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Utils;

public class CreateVisitRequest : BaseCreateRequest
{
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("visitedOn")]
    public string? VisitedOn { get; set; }

    [JsonPropertyName("cost")]
    public double? Cost { get; set; }

    public VisitEntity ToEntity()
    {
        var item = new VisitEntity();
        ApplyTo(item);
        item.Reason = Reason;
        item.VisitedOn = VisitedOn;
        item.Cost = Cost;
        item.Paid = false;
        return item;
    }
}

public class UpdateVisitRequest : BaseUpdateRequest
{
    [JsonPropertyName("visitedOn")]
    public string? VisitedOn { get; set; }

    [JsonPropertyName("cost")]
    public double? Cost { get; set; }

    [JsonPropertyName("paid")]
    public bool? Paid { get; set; }

    [JsonPropertyName("checkedAt")]
    public List<string>? CheckedAt { get; set; }

    public void ApplyTo(VisitEntity item)
    {
        base.ApplyTo(item);
        if (Sent.Contains("visitedOn"))
        {
            item.VisitedOn = VisitedOn;
        }
        if (Sent.Contains("cost"))
        {
            item.Cost = Cost;
        }
        if (Sent.Contains("paid"))
        {
            item.Paid = Paid;
        }
        if (Sent.Contains("checkedAt"))
        {
            item.CheckedAt = Fields.UtcDateTimes(CheckedAt);
        }
    }
}
