namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;

public interface I{{project_class_name}}Controller
{
    Task<{{project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<{{project_class_name}}Dto>> GetListAsync(CancellationToken ct = default);

    Task<{{project_class_name}}Dto> CreateAsync(Stream item, CancellationToken ct = default);

    Task<{{project_class_name}}Dto> UpdateAsync(string id, Stream item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
