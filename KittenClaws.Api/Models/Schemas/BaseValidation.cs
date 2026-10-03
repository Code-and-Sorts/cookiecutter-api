namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;

public class BaseCreateRequestValidator : AbstractValidator<BaseCreateRequest>
{
    public BaseCreateRequestValidator()
    {
    }
}

public class BaseUpdateRequestValidator : AbstractValidator<BaseUpdateRequest>
{
    public BaseUpdateRequestValidator()
    {
    }
}
