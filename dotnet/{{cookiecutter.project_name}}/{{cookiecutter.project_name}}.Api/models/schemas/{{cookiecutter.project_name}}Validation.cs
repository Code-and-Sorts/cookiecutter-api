namespace {{cookiecutter.project_name}}.Api.Validation;

using FluentValidation;
using {{cookiecutter.project_name}}.Api.Requests;

public class Create{{cookiecutter.project_name}}RequestValidator : AbstractValidator<Create{{cookiecutter.project_name}}Request>
{
    public Create{{cookiecutter.project_name}}RequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class Update{{cookiecutter.project_name}}RequestValidator : AbstractValidator<Update{{cookiecutter.project_name}}Request>
{
    public Update{{cookiecutter.project_name}}RequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}
