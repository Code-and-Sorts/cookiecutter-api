namespace {{cookiecutter.project_class_name}}.Api.Requests;

using System.Text.Json.Serialization;

public class Create{{cookiecutter.project_class_name}}Request
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; } = default!;

    [JsonPropertyName("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}

public class Update{{cookiecutter.project_class_name}}Request
{
    [JsonIgnore]
    public string Id { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("updatedBy")]
    public string UpdatedBy { get; set; } = default!;
}
