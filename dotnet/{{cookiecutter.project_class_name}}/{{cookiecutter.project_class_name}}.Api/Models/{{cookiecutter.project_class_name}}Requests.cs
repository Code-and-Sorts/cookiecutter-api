namespace {{cookiecutter.project_class_name}}.Api.Requests;

using Newtonsoft.Json;

public class Create{{cookiecutter.project_class_name}}Request
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}

public class Update{{cookiecutter.project_class_name}}Request
{
    [JsonIgnore]
    public string Id { get; set; } = default!;

    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}
