{%- from 'dotnet/_model.jinja' import csharp_file, properties, shown_assignments, usings with context -%}
{%- set shown = client_base_fields | rejectattr("hidden") | list -%}
{%- set body %}
public class BaseResponse
{
    public BaseResponse()
    {
    }

    public BaseResponse(BaseEntity entity)
    {
        Id = entity.Id;
{{- shown_assignments(shown, "entity") }}
    }

    // Before the resource's own fields, which System.Text.Json would otherwise write first.
    [JsonPropertyName("id")]
    [JsonPropertyOrder(-1)]
    public string Id { get; set; } = default!;
{%- if shown %}
{{ properties(shown, response=true, order=-1) }}
{%- endif %}
}
{%- endset -%}
{{ csharp_file("Dtos", usings("response", shown, base=true), body) }}
