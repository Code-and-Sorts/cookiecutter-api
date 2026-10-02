namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;

public class CreateDogRequestValidator : AbstractValidator<CreateDogRequest>
{
    public CreateDogRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("name is required and must be a non-empty string.");
    }
}

public class ReplaceDogRequestValidator : AbstractValidator<ReplaceDogRequest>
{
    public ReplaceDogRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("name is required and must be a non-empty string.");
    }
}
