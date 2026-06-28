namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;

public interface I{{project_class_name}}Repository
{
    Task<{{project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<{{project_class_name}}Dto>> GetListAsync(CancellationToken ct = default);

    Task<{{project_class_name}}Dto> CreateAsync({{project_class_name}} {{project_lower_camel_name}}, CancellationToken ct = default);

    Task<{{project_class_name}}Dto> UpdateAsync({{project_class_name}} {{project_lower_camel_name}}, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
