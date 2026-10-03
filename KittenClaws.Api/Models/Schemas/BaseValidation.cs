namespace KittenClaws.Api.Validation;

using System.Linq;
using FluentValidation;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;

public class BaseCreateRequestValidator : AbstractValidator<BaseCreateRequest>
{
    public BaseCreateRequestValidator()
    {
        RuleFor(x => x.TenantId).NotNull().When(x => x.Sent.Contains("tenantId")).WithMessage("tenantId must not be null.");
        RuleFor(x => x.TenantId).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("tenantId must be at least 1 character long.");
        RuleFor(x => x.TenantId).Must(value => value == null || Fields.Length(value) <= 64).WithMessage("tenantId must be at most 64 characters long.");
        RuleFor(x => x.Region).NotNull().When(x => x.Sent.Contains("region")).WithMessage("region must not be null.");
        RuleFor(x => x.Region).Must(value => value is null or "eu" or "us").WithMessage("region must be one of eu, us.");
        RuleFor(x => x.Priority).Must(Fields.IsSafeInteger).WithMessage("priority must be an integer between -9007199254740991 and 9007199254740991.");
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0L).WithMessage("priority must be at least 0.");
        RuleFor(x => x.Sent).Must(sent => sent.Contains("rank")).WithMessage("rank is required.");
        RuleFor(x => x.Rank).NotNull().When(x => x.Sent.Contains("rank")).WithMessage("rank must not be null.");
        RuleFor(x => x.Labels).NotNull().When(x => x.Sent.Contains("labels")).WithMessage("labels must not be null.");
        RuleFor(x => x.Labels).Must(values => values == null || values.Distinct().Count() == values.Count).WithMessage("labels must not repeat an item.");
    }
}

public class BaseReplaceRequestValidator : AbstractValidator<BaseReplaceRequest>
{
    public BaseReplaceRequestValidator()
    {
        RuleFor(x => x.TenantId).NotNull().When(x => x.Sent.Contains("tenantId")).WithMessage("tenantId must not be null.");
        RuleFor(x => x.TenantId).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("tenantId must be at least 1 character long.");
        RuleFor(x => x.TenantId).Must(value => value == null || Fields.Length(value) <= 64).WithMessage("tenantId must be at most 64 characters long.");
        RuleFor(x => x.Priority).Must(Fields.IsSafeInteger).WithMessage("priority must be an integer between -9007199254740991 and 9007199254740991.");
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0L).WithMessage("priority must be at least 0.");
        RuleFor(x => x.Sent).Must(sent => sent.Contains("rank")).WithMessage("rank is required.");
        RuleFor(x => x.Rank).NotNull().When(x => x.Sent.Contains("rank")).WithMessage("rank must not be null.");
        RuleFor(x => x.Labels).NotNull().When(x => x.Sent.Contains("labels")).WithMessage("labels must not be null.");
        RuleFor(x => x.Labels).Must(values => values == null || values.Distinct().Count() == values.Count).WithMessage("labels must not repeat an item.");
    }
}

public class BaseUpdateRequestValidator : AbstractValidator<BaseUpdateRequest>
{
    public BaseUpdateRequestValidator()
    {
        RuleFor(x => x.TenantId).NotNull().When(x => x.Sent.Contains("tenantId")).WithMessage("tenantId must not be null.");
        RuleFor(x => x.TenantId).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("tenantId must be at least 1 character long.");
        RuleFor(x => x.TenantId).Must(value => value == null || Fields.Length(value) <= 64).WithMessage("tenantId must be at most 64 characters long.");
        RuleFor(x => x.Priority).Must(Fields.IsSafeInteger).WithMessage("priority must be an integer between -9007199254740991 and 9007199254740991.");
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0L).WithMessage("priority must be at least 0.");
        RuleFor(x => x.Rank).NotNull().When(x => x.Sent.Contains("rank")).WithMessage("rank must not be null.");
        RuleFor(x => x.Labels).NotNull().When(x => x.Sent.Contains("labels")).WithMessage("labels must not be null.");
        RuleFor(x => x.Labels).Must(values => values == null || values.Distinct().Count() == values.Count).WithMessage("labels must not repeat an item.");
    }
}
