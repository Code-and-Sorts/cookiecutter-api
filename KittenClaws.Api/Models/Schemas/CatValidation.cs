namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;

public class CreateCatRequestValidator : AbstractValidator<CreateCatRequest>
{
    public CreateCatRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("name is required and must be a non-empty string.");
    }
}

public class UpdateCatRequestValidator : AbstractValidator<UpdateCatRequest>
{
    public UpdateCatRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().When(x => x.Name != null).WithMessage("name must be a non-empty string.");
    }
}
