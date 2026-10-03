namespace KittenClaws.Api.Entities;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class BaseEntity
{
    public string Id { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;

    public string CreatedTimestamp { get; set; } = default!;

    public string UpdatedTimestamp { get; set; } = default!;

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    /// <summary>Owning tenant</summary>
    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("priority")]
    public long? Priority { get; set; }

    [JsonPropertyName("rank")]
    public double? Rank { get; set; }

    [JsonPropertyName("labels")]
    public List<string>? Labels { get; set; }
}
