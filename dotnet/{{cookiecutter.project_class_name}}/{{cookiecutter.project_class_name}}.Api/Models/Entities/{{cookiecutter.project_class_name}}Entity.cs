namespace {{cookiecutter.project_class_name}}.Api.Entities;

using Newtonsoft.Json;

public class {{cookiecutter.project_class_name}} : BaseEntity
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;
}
