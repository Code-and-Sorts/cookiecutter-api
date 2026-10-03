{%- from 'dotnet/_model.jinja' import csharp_file, properties with context -%}
{%- set user_fields = base_fields | rejectattr("system") | list -%}
{%- set body %}
public class BaseEntity
{
    public string Id { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;

    public string CreatedTimestamp { get; set; } = default!;

    public string UpdatedTimestamp { get; set; } = default!;

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
{%- if user_fields %}
{{ properties(user_fields) }}
{%- endif %}
}
{%- endset -%}
{{ csharp_file("Entities", body) }}
