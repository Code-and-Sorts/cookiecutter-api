namespace KittenClaws.Api.Dtos;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;

public class BaseResponse
{
    public BaseResponse()
    {
    }

    public BaseResponse(BaseEntity entity)
    {
        Id = entity.Id;
        TenantId = entity.TenantId ?? "public";
        Region = entity.Region ?? "eu";
        Priority = entity.Priority;
        Rank = entity.Rank;
        Labels = entity.Labels ?? [];
    }

    // Before the resource's own fields, which System.Text.Json would otherwise write first.
    [JsonPropertyName("id")]
    [JsonPropertyOrder(-1)]
    public string Id { get; set; } = default!;

    /// <summary>Owning tenant</summary>
    [JsonPropertyName("tenantId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyOrder(-1)]
    public string? TenantId { get; set; }

    [JsonPropertyName("region")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyOrder(-1)]
    public string? Region { get; set; }

    [JsonPropertyName("priority")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyOrder(-1)]
    public long? Priority { get; set; }

    [JsonPropertyName("rank")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyOrder(-1)]
    public double? Rank { get; set; }

    [JsonPropertyName("labels")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyOrder(-1)]
    public List<string>? Labels { get; set; }
}
