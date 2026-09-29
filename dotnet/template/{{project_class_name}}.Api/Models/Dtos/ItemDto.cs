namespace {{project_class_name}}.Api.Dtos;
{% for resource in resources %}
public class {{ resource.name }}Dto
{
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string UpdatedBy { get; set; } = default!;
}
{% endfor %}