namespace {{cookiecutter.project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_class_name}}.Api.Dtos;
using {{cookiecutter.project_class_name}}.Api.Entities;

public interface I{{cookiecutter.project_class_name}}Repository
{
    Task<{{cookiecutter.project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<{{cookiecutter.project_class_name}}Dto>> GetListAsync(CancellationToken ct = default);

    Task<{{cookiecutter.project_class_name}}Dto> CreateAsync({{cookiecutter.project_class_name}} {{cookiecutter.project_lower_camel_name}}, CancellationToken ct = default);

    Task<{{cookiecutter.project_class_name}}Dto> UpdateAsync({{cookiecutter.project_class_name}} {{cookiecutter.project_lower_camel_name}}, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
