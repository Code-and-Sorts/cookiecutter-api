namespace KittenClaws.Api.Requests;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Utils;

public class BaseCreateRequest : ISentFields
{
    [JsonIgnore]
    public HashSet<string> Sent { get; } = [];

    public void ApplyTo(BaseEntity entity)
    {
    }
}

public class BaseUpdateRequest : ISentFields
{
    [JsonIgnore]
    public HashSet<string> Sent { get; } = [];

    [JsonIgnore]
    public string Id { get; set; } = default!;

    public void ApplyTo(BaseEntity entity)
    {
    }
}
