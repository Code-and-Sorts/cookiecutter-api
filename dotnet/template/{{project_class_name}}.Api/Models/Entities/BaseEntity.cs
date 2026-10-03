{%- from 'dotnet/_model.jinja' import csharp_file, properties, usings with context -%}
{%- set body %}
public class BaseEntity
{
    public string Id { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;

    public string CreatedTimestamp { get; set; } = default!;

    public string UpdatedTimestamp { get; set; } = default!;

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
{%- if client_base_fields %}
{{ properties(client_base_fields) }}
{%- endif %}
}
{%- endset -%}
{{ csharp_file("Entities", usings("entity", client_base_fields), body) }}
