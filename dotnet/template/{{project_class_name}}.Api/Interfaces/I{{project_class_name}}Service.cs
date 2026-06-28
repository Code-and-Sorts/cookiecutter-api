namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Requests;

public interface I{{project_class_name}}Service
{
    Task<{{project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<{{project_class_name}}Dto>> GetListAsync(CancellationToken ct = default);

    Task<{{project_class_name}}Dto> CreateAsync(Create{{project_class_name}}Request item, CancellationToken ct = default);

    Task<{{project_class_name}}Dto> UpdateAsync(Update{{project_class_name}}Request item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
