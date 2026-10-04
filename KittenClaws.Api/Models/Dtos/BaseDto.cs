namespace KittenClaws.Api.Dtos;

using System.Text.Json.Serialization;

public class BaseDto
{
    [JsonPropertyName("id")]
    [JsonPropertyOrder(-1)]
    public string Id { get; set; } = default!;

    // Resource fields keep the default order 0, so they sit between id and these.
    [JsonPropertyName("createdTimestamp")]
    [JsonPropertyOrder(1)]
    public string CreatedTimestamp { get; set; } = default!;

    [JsonPropertyName("createdBy")]
    [JsonPropertyOrder(2)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updatedTimestamp")]
    [JsonPropertyOrder(3)]
    public string UpdatedTimestamp { get; set; } = default!;

    [JsonPropertyName("updatedBy")]
    [JsonPropertyOrder(4)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UpdatedBy { get; set; }
}
