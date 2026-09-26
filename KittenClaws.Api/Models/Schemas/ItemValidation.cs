namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;

public class CreateCatRequestValidator : AbstractValidator<CreateCatRequest>
{
    public CreateCatRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class UpdateCatRequestValidator : AbstractValidator<UpdateCatRequest>
{
    public UpdateCatRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class CreateDogRequestValidator : AbstractValidator<CreateDogRequest>
{
    public CreateDogRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class ReplaceDogRequestValidator : AbstractValidator<ReplaceDogRequest>
{
    public ReplaceDogRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}
