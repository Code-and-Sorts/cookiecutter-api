namespace KittenClaws.Api.Entities;

using System;
using Newtonsoft.Json;
public class BaseEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [JsonProperty("createdTimestamp")]
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedTimestamp")]
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}
