namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;
{% for resource in resources %}
public interface I{{ resource.name }}Repository
{
    Task<{{ resource.name }}Dto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<{{ resource.name }}Dto>> GetListAsync(CancellationToken ct = default);

    Task<{{ resource.name }}Dto> CreateAsync({{ resource.name }} item, CancellationToken ct = default);

    Task<{{ resource.name }}Dto> UpdateAsync({{ resource.name }} item, CancellationToken ct = default);

    Task<{{ resource.name }}Dto> ReplaceAsync({{ resource.name }} item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
{% endfor %}