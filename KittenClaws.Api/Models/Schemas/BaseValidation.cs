namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;

public class BaseCreateRequestValidator : AbstractValidator<BaseCreateRequest>
{
    public BaseCreateRequestValidator()
    {
    }
}

public class BaseReplaceRequestValidator : AbstractValidator<BaseReplaceRequest>
{
    public BaseReplaceRequestValidator()
    {
    }
}

public class BaseUpdateRequestValidator : AbstractValidator<BaseUpdateRequest>
{
    public BaseUpdateRequestValidator()
    {
    }
}
