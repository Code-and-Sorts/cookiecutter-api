namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;

public class CreateKittenClawsRequestValidator : AbstractValidator<CreateKittenClawsRequest>
{
    public CreateKittenClawsRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class UpdateKittenClawsRequestValidator : AbstractValidator<UpdateKittenClawsRequest>
{
    public UpdateKittenClawsRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}
