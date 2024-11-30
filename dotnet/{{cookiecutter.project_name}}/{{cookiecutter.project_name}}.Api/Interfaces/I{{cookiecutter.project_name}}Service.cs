namespace {{cookiecutter.project_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_name}}.Api.Dtos;
using {{cookiecutter.project_name}}.Api.Requests;

public interface I{{cookiecutter.project_name}}Service
{
    Task<{{cookiecutter.project_name}}Dto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<{{cookiecutter.project_name}}Dto>> GetListAsync(CancellationToken ct = default);

    Task<{{cookiecutter.project_name}}Dto> CreateAsync(Create{{cookiecutter.project_name}}Request item, CancellationToken ct = default);

    Task<{{cookiecutter.project_name}}Dto> UpdateAsync(Update{{cookiecutter.project_name}}Request item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
