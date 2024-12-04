namespace {{cookiecutter.project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_class_name}}.Api.Dtos;

public interface I{{cookiecutter.project_class_name}}Controller
{
    Task<{{cookiecutter.project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<{{cookiecutter.project_class_name}}Dto>> GetListAsync(CancellationToken ct = default);

    Task<{{cookiecutter.project_class_name}}Dto> CreateAsync(Stream item, CancellationToken ct = default);

    Task<{{cookiecutter.project_class_name}}Dto> UpdateAsync(string id, Stream item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
