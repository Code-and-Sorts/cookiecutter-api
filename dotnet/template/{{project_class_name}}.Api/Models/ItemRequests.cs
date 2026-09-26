namespace {{project_class_name}}.Api.Requests;

using Newtonsoft.Json;
{% for resource in resources %}
{%- if "create" in resource.operations %}
public class Create{{ resource.name }}Request
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}
{% endif %}
{%- if "update" in resource.operations %}
public class Update{{ resource.name }}Request
{
    [JsonIgnore]
    public string Id { get; set; } = default!;

    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}
{% endif %}
{%- if "replace" in resource.operations %}
public class Replace{{ resource.name }}Request
{
    [JsonIgnore]
    public string Id { get; set; } = default!;

    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}
{% endif %}
{%- endfor %}