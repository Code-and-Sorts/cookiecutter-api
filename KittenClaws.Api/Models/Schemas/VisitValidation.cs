namespace KittenClaws.Api.Validation;

using FluentValidation;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;

public class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    public CreateVisitRequestValidator()
    {
        Include(new BaseCreateRequestValidator());
        RuleFor(x => x.Sent).Must(sent => sent.Contains("reason")).WithMessage("reason is required.");
        RuleFor(x => x.Reason).NotNull().When(x => x.Sent.Contains("reason")).WithMessage("reason must not be null.");
        RuleFor(x => x.Sent).Must(sent => sent.Contains("visitedOn")).WithMessage("visitedOn is required.");
        RuleFor(x => x.VisitedOn).NotNull().When(x => x.Sent.Contains("visitedOn")).WithMessage("visitedOn must not be null.");
        RuleFor(x => x.VisitedOn).Must(Fields.IsDate).WithMessage("visitedOn must be a date such as 2026-01-31.");
        RuleFor(x => x.Cost).NotNull().When(x => x.Sent.Contains("cost")).WithMessage("cost must not be null.");
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0d).WithMessage("cost must be at least 0.");
    }
}

public class UpdateVisitRequestValidator : AbstractValidator<UpdateVisitRequest>
{
    public UpdateVisitRequestValidator()
    {
        Include(new BaseUpdateRequestValidator());
        RuleFor(x => x.VisitedOn).NotNull().When(x => x.Sent.Contains("visitedOn")).WithMessage("visitedOn must not be null.");
        RuleFor(x => x.VisitedOn).Must(Fields.IsDate).WithMessage("visitedOn must be a date such as 2026-01-31.");
        RuleFor(x => x.Cost).NotNull().When(x => x.Sent.Contains("cost")).WithMessage("cost must not be null.");
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0d).WithMessage("cost must be at least 0.");
        RuleFor(x => x.Paid).NotNull().When(x => x.Sent.Contains("paid")).WithMessage("paid must not be null.");
        RuleFor(x => x.CheckedAt).NotNull().When(x => x.Sent.Contains("checkedAt")).WithMessage("checkedAt must not be null.");
        RuleFor(x => x.CheckedAt).Must(values => values == null || values.TrueForAll(value => Fields.IsDateTime(value))).WithMessage("checkedAt items must each be a date-time with an offset, such as 2026-01-31T09:30:00Z.");
    }
}
