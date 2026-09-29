namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Requests;
{% for resource in resources %}
public interface I{{ resource.name }}Service
{
{%- if "get_by_id" in resource.operations %}
    Task<{{ resource.name }}Dto> GetAsync(string id, CancellationToken ct = default);
{%- endif %}
{%- if "list" in resource.operations %}
    Task<IEnumerable<{{ resource.name }}Dto>> GetListAsync(CancellationToken ct = default);
{%- endif %}
{%- if "create" in resource.operations %}
    Task<{{ resource.name }}Dto> CreateAsync(Create{{ resource.name }}Request item, CancellationToken ct = default);
{%- endif %}
{%- if "update" in resource.operations %}
    Task<{{ resource.name }}Dto> UpdateAsync(Update{{ resource.name }}Request item, CancellationToken ct = default);
{%- endif %}
{%- if "replace" in resource.operations %}
    Task<{{ resource.name }}Dto> ReplaceAsync(Replace{{ resource.name }}Request item, CancellationToken ct = default);
{%- endif %}
{%- if "delete" in resource.operations %}
    Task DeleteAsync(string id, CancellationToken ct = default);
{%- endif %}
}
{% endfor %}