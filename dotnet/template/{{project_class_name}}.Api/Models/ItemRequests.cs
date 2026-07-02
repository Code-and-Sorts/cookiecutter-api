namespace {{project_class_name}}.Api.Requests;

using Newtonsoft.Json;

public class CreateItemRequest
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}

public class UpdateItemRequest
{
    [JsonIgnore]
    public string Id { get; set; } = default!;

    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}

public class ReplaceItemRequest
{
    [JsonIgnore]
    public string Id { get; set; } = default!;

    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}
