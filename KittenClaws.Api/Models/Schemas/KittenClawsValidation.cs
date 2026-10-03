namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;

public class CreateKittenClawsRequestValidator : AbstractValidator<CreateKittenClawsRequest>
{
    public CreateKittenClawsRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("name is required and must be a non-empty string.");
    }
}

public class UpdateKittenClawsRequestValidator : AbstractValidator<UpdateKittenClawsRequest>
{
    public UpdateKittenClawsRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().When(x => x.Name != null).WithMessage("name must be a non-empty string.");
    }
}
