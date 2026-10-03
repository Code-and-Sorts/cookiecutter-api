namespace KittenClaws.Api.Requests;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Utils;

public class BaseCreateRequest : ISentFields
{
    [JsonIgnore]
    public HashSet<string> Sent { get; } = [];

    /// <summary>Owning tenant</summary>
    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; } = "public";

    [JsonPropertyName("region")]
    public string? Region { get; set; } = "eu";

    [JsonPropertyName("priority")]
    public long? Priority { get; set; }

    [JsonPropertyName("rank")]
    public double? Rank { get; set; }

    [JsonPropertyName("labels")]
    public List<string>? Labels { get; set; } = [];

    public void ApplyTo(BaseEntity entity)
    {
        entity.TenantId = TenantId;
        entity.Region = Region;
        entity.Priority = Priority;
        entity.Rank = Rank;
        entity.Labels = Labels;
    }
}

public class BaseReplaceRequest : ISentFields
{
    [JsonIgnore]
    public HashSet<string> Sent { get; } = [];

    [JsonIgnore]
    public string Id { get; set; } = default!;

    /// <summary>Owning tenant</summary>
    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; } = "public";

    [JsonPropertyName("priority")]
    public long? Priority { get; set; }

    [JsonPropertyName("rank")]
    public double? Rank { get; set; }

    [JsonPropertyName("labels")]
    public List<string>? Labels { get; set; } = [];

    public void ApplyTo(BaseEntity entity)
    {
        entity.TenantId = TenantId;
        entity.Priority = Priority;
        entity.Rank = Rank;
        entity.Labels = Labels;
    }
}

public class BaseUpdateRequest : ISentFields
{
    [JsonIgnore]
    public HashSet<string> Sent { get; } = [];

    [JsonIgnore]
    public string Id { get; set; } = default!;

    /// <summary>Owning tenant</summary>
    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }

    [JsonPropertyName("priority")]
    public long? Priority { get; set; }

    [JsonPropertyName("rank")]
    public double? Rank { get; set; }

    [JsonPropertyName("labels")]
    public List<string>? Labels { get; set; }

    public void ApplyTo(BaseEntity entity)
    {
        if (Sent.Contains("tenantId"))
        {
            entity.TenantId = TenantId;
        }
        if (Sent.Contains("priority"))
        {
            entity.Priority = Priority;
        }
        if (Sent.Contains("rank"))
        {
            entity.Rank = Rank;
        }
        if (Sent.Contains("labels"))
        {
            entity.Labels = Labels;
        }
    }
}
