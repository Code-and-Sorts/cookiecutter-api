namespace {{cookiecutter.project_class_name}}.Api.Controllers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_class_name}}.Api.Dtos;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Requests;

public class {{cookiecutter.project_class_name}}Controller : I{{cookiecutter.project_class_name}}Controller
{
    private readonly I{{cookiecutter.project_class_name}}Service _{{cookiecutter.project_lower_camel_name}}Service;

    public {{cookiecutter.project_class_name}}Controller(I{{cookiecutter.project_class_name}}Service {{cookiecutter.project_lower_camel_name}}Service)
    {
        _{{cookiecutter.project_lower_camel_name}}Service = {{cookiecutter.project_lower_camel_name}}Service;
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Service.GetAsync(id, ct);

    public async Task<IEnumerable<{{cookiecutter.project_class_name}}Dto>> GetListAsync(CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Service.GetListAsync(ct);

    public async Task<{{cookiecutter.project_class_name}}Dto> CreateAsync(Create{{cookiecutter.project_class_name}}Request {{cookiecutter.project_lower_camel_name}}, CancellationToken ct = default)
    {
        // TODO: Add validation
        return await _{{cookiecutter.project_lower_camel_name}}Service.CreateAsync({{cookiecutter.project_lower_camel_name}}, ct);
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> UpdateAsync(Update{{cookiecutter.project_class_name}}Request {{cookiecutter.project_lower_camel_name}}, CancellationToken ct = default)
    {
        // TODO: Add validation
        return await _{{cookiecutter.project_lower_camel_name}}Service.UpdateAsync({{cookiecutter.project_lower_camel_name}}, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Service.DeleteAsync(id, ct);
}
