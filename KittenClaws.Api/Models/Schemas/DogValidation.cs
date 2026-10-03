namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;

public class CreateDogRequestValidator : AbstractValidator<CreateDogRequest>
{
    public CreateDogRequestValidator()
    {
        Include(new BaseCreateRequestValidator());
        RuleFor(x => x.Sent).Must(sent => sent.Contains("name")).WithMessage("name is required.");
        RuleFor(x => x.Name).NotNull().When(x => x.Sent.Contains("name")).WithMessage("name must not be null.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("name must be at least 1 character long.");
    }
}

public class ReplaceDogRequestValidator : AbstractValidator<ReplaceDogRequest>
{
    public ReplaceDogRequestValidator()
    {
        Include(new BaseReplaceRequestValidator());
        RuleFor(x => x.Sent).Must(sent => sent.Contains("name")).WithMessage("name is required.");
        RuleFor(x => x.Name).NotNull().When(x => x.Sent.Contains("name")).WithMessage("name must not be null.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("name must be at least 1 character long.");
    }
}
