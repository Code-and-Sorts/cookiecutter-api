namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;

public class CreateKittenClawsRequestValidator : AbstractValidator<CreateKittenClawsRequest>
{
    public CreateKittenClawsRequestValidator()
    {
        Include(new BaseCreateRequestValidator());
        RuleFor(x => x.Sent).Must(sent => sent.Contains("name")).WithMessage("name is required.");
        RuleFor(x => x.Name).NotNull().When(x => x.Sent.Contains("name")).WithMessage("name must not be null.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("name must be at least 1 character long.");
    }
}

public class UpdateKittenClawsRequestValidator : AbstractValidator<UpdateKittenClawsRequest>
{
    public UpdateKittenClawsRequestValidator()
    {
        Include(new BaseUpdateRequestValidator());
        RuleFor(x => x.Name).NotNull().When(x => x.Sent.Contains("name")).WithMessage("name must not be null.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("name must be at least 1 character long.");
    }
}
