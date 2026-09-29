{%- set used_ops = resources | map(attribute='operations') | sum(start=[]) -%}
{%- set uses_body = ('create' in used_ops) or ('update' in used_ops) or ('replace' in used_ops) -%}
namespace {{project_class_name}}.Api.Validation;
{% if uses_body %}
using FluentValidation;
using {{project_class_name}}.Api.Requests;
{% endif %}
{%- for resource in resources %}
{%- if "create" in resource.operations %}
public class Create{{ resource.name }}RequestValidator : AbstractValidator<Create{{ resource.name }}Request>
{
    public Create{{ resource.name }}RequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}
{% endif %}
{%- if "update" in resource.operations %}
public class Update{{ resource.name }}RequestValidator : AbstractValidator<Update{{ resource.name }}Request>
{
    public Update{{ resource.name }}RequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}
{% endif %}
{%- if "replace" in resource.operations %}
public class Replace{{ resource.name }}RequestValidator : AbstractValidator<Replace{{ resource.name }}Request>
{
    public Replace{{ resource.name }}RequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}
{% endif %}
{%- endfor %}