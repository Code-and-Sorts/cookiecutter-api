namespace {{project_class_name}}.Api.Validation;

using FluentValidation;
using {{project_class_name}}.Api.Requests;

public class Create{{project_class_name}}RequestValidator : AbstractValidator<Create{{project_class_name}}Request>
{
    public Create{{project_class_name}}RequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class Update{{project_class_name}}RequestValidator : AbstractValidator<Update{{project_class_name}}Request>
{
    public Update{{project_class_name}}RequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}
