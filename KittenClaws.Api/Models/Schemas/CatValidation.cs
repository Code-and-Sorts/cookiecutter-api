namespace KittenClaws.Api.Validation;

using System.Linq;
using FluentValidation;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;

public class CreateCatRequestValidator : AbstractValidator<CreateCatRequest>
{
    public CreateCatRequestValidator()
    {
        Include(new BaseCreateRequestValidator());
        RuleFor(x => x.Sent).Must(sent => sent.Contains("name")).WithMessage("name is required.");
        RuleFor(x => x.Name).NotNull().When(x => x.Sent.Contains("name")).WithMessage("name must not be null.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("name must be at least 1 character long.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) <= 100).WithMessage("name must be at most 100 characters long.");
        RuleFor(x => x.Breed).NotNull().When(x => x.Sent.Contains("breed")).WithMessage("breed must not be null.");
        RuleFor(x => x.Breed).Must(value => value is null or "siamese" or "persian" or "tabby").WithMessage("breed must be one of siamese, persian, tabby.");
        RuleFor(x => x.AgeYears).NotNull().When(x => x.Sent.Contains("ageYears")).WithMessage("ageYears must not be null.");
        RuleFor(x => x.AgeYears).Must(Fields.IsSafeInteger).WithMessage("ageYears must be an integer between -9007199254740991 and 9007199254740991.");
        RuleFor(x => x.AgeYears).GreaterThanOrEqualTo(0L).WithMessage("ageYears must be at least 0.");
        RuleFor(x => x.AgeYears).LessThanOrEqualTo(40L).WithMessage("ageYears must be at most 40.");
        RuleFor(x => x.WeightKg).GreaterThan(0d).WithMessage("weightKg must be greater than 0.");
        RuleFor(x => x.WeightKg).LessThan(100d).WithMessage("weightKg must be less than 100.");
        RuleFor(x => x.Indoor).NotNull().When(x => x.Sent.Contains("indoor")).WithMessage("indoor must not be null.");
        RuleFor(x => x.BirthDate).NotNull().When(x => x.Sent.Contains("birthDate")).WithMessage("birthDate must not be null.");
        RuleFor(x => x.BirthDate).Must(Fields.IsDate).WithMessage("birthDate must be a date such as 2026-01-31.");
        RuleFor(x => x.MicrochipId).NotNull().When(x => x.Sent.Contains("microchipId")).WithMessage("microchipId must not be null.");
        RuleFor(x => x.MicrochipId).Must(Fields.IsUuid).WithMessage("microchipId must be a UUID.");
        RuleFor(x => x.OwnerEmail).NotNull().When(x => x.Sent.Contains("ownerEmail")).WithMessage("ownerEmail must not be null.");
        RuleFor(x => x.OwnerEmail).Must(value => value == null || Fields.Matches(value, @"^[^@ \t\n]+@[^@ \t\n]+\.[^@ \t\n]+\z")).WithMessage("ownerEmail must be an email address.");
        RuleFor(x => x.Website).Must(value => value == null || Fields.Matches(value, @"^[A-Za-z][A-Za-z0-9+.-]*:[^ \t\n]+\z")).WithMessage("website must be an absolute URI.");
        RuleFor(x => x.TagCode).NotNull().When(x => x.Sent.Contains("tagCode")).WithMessage("tagCode must not be null.");
        RuleFor(x => x.TagCode).Must(value => value == null || Fields.Matches(value, @"^[A-Z]{3}-[0-9]{3}\z")).WithMessage("tagCode must match the pattern ^[A-Z]{3}-[0-9]{3}$.");
        RuleFor(x => x.Tags).NotNull().When(x => x.Sent.Contains("tags")).WithMessage("tags must not be null.");
        RuleFor(x => x.Tags).Must(values => values == null || values.Count <= 3).WithMessage("tags must have at most 3 items.");
        RuleFor(x => x.Tags).Must(values => values == null || values.Distinct().Count() == values.Count).WithMessage("tags must not repeat an item.");
        RuleFor(x => x.Scores).Must(values => values == null || values.TrueForAll(value => Fields.IsSafeInteger(value))).WithMessage("scores items must each be an integer between -9007199254740991 and 9007199254740991.");
        RuleFor(x => x.Scores).Must(values => values == null || values.Count >= 1).WithMessage("scores must have at least 1 item.");
        RuleFor(x => x.AdoptedAt).Must(Fields.IsDateTime).WithMessage("adoptedAt must be a date-time with an offset, such as 2026-01-31T09:30:00Z.");
        RuleFor(x => x.LastVisit).NotNull().When(x => x.Sent.Contains("lastVisit")).WithMessage("lastVisit must not be null.");
        RuleFor(x => x.LastVisit).Must(Fields.IsDateTime).WithMessage("lastVisit must be a date-time with an offset, such as 2026-01-31T09:30:00Z.");
        RuleFor(x => x.Notes).NotNull().When(x => x.Sent.Contains("notes")).WithMessage("notes must not be null.");
    }
}

public class ReplaceCatRequestValidator : AbstractValidator<ReplaceCatRequest>
{
    public ReplaceCatRequestValidator()
    {
        Include(new BaseReplaceRequestValidator());
        RuleFor(x => x.Sent).Must(sent => sent.Contains("name")).WithMessage("name is required.");
        RuleFor(x => x.Name).NotNull().When(x => x.Sent.Contains("name")).WithMessage("name must not be null.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("name must be at least 1 character long.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) <= 100).WithMessage("name must be at most 100 characters long.");
        RuleFor(x => x.Breed).NotNull().When(x => x.Sent.Contains("breed")).WithMessage("breed must not be null.");
        RuleFor(x => x.Breed).Must(value => value is null or "siamese" or "persian" or "tabby").WithMessage("breed must be one of siamese, persian, tabby.");
        RuleFor(x => x.AgeYears).NotNull().When(x => x.Sent.Contains("ageYears")).WithMessage("ageYears must not be null.");
        RuleFor(x => x.AgeYears).Must(Fields.IsSafeInteger).WithMessage("ageYears must be an integer between -9007199254740991 and 9007199254740991.");
        RuleFor(x => x.AgeYears).GreaterThanOrEqualTo(0L).WithMessage("ageYears must be at least 0.");
        RuleFor(x => x.AgeYears).LessThanOrEqualTo(40L).WithMessage("ageYears must be at most 40.");
        RuleFor(x => x.WeightKg).GreaterThan(0d).WithMessage("weightKg must be greater than 0.");
        RuleFor(x => x.WeightKg).LessThan(100d).WithMessage("weightKg must be less than 100.");
        RuleFor(x => x.Indoor).NotNull().When(x => x.Sent.Contains("indoor")).WithMessage("indoor must not be null.");
        RuleFor(x => x.BirthDate).NotNull().When(x => x.Sent.Contains("birthDate")).WithMessage("birthDate must not be null.");
        RuleFor(x => x.BirthDate).Must(Fields.IsDate).WithMessage("birthDate must be a date such as 2026-01-31.");
        RuleFor(x => x.OwnerEmail).NotNull().When(x => x.Sent.Contains("ownerEmail")).WithMessage("ownerEmail must not be null.");
        RuleFor(x => x.OwnerEmail).Must(value => value == null || Fields.Matches(value, @"^[^@ \t\n]+@[^@ \t\n]+\.[^@ \t\n]+\z")).WithMessage("ownerEmail must be an email address.");
        RuleFor(x => x.Website).Must(value => value == null || Fields.Matches(value, @"^[A-Za-z][A-Za-z0-9+.-]*:[^ \t\n]+\z")).WithMessage("website must be an absolute URI.");
        RuleFor(x => x.TagCode).NotNull().When(x => x.Sent.Contains("tagCode")).WithMessage("tagCode must not be null.");
        RuleFor(x => x.TagCode).Must(value => value == null || Fields.Matches(value, @"^[A-Z]{3}-[0-9]{3}\z")).WithMessage("tagCode must match the pattern ^[A-Z]{3}-[0-9]{3}$.");
        RuleFor(x => x.Tags).NotNull().When(x => x.Sent.Contains("tags")).WithMessage("tags must not be null.");
        RuleFor(x => x.Tags).Must(values => values == null || values.Count <= 3).WithMessage("tags must have at most 3 items.");
        RuleFor(x => x.Tags).Must(values => values == null || values.Distinct().Count() == values.Count).WithMessage("tags must not repeat an item.");
        RuleFor(x => x.Scores).Must(values => values == null || values.TrueForAll(value => Fields.IsSafeInteger(value))).WithMessage("scores items must each be an integer between -9007199254740991 and 9007199254740991.");
        RuleFor(x => x.Scores).Must(values => values == null || values.Count >= 1).WithMessage("scores must have at least 1 item.");
        RuleFor(x => x.AdoptedAt).Must(Fields.IsDateTime).WithMessage("adoptedAt must be a date-time with an offset, such as 2026-01-31T09:30:00Z.");
        RuleFor(x => x.LastVisit).NotNull().When(x => x.Sent.Contains("lastVisit")).WithMessage("lastVisit must not be null.");
        RuleFor(x => x.LastVisit).Must(Fields.IsDateTime).WithMessage("lastVisit must be a date-time with an offset, such as 2026-01-31T09:30:00Z.");
        RuleFor(x => x.Notes).NotNull().When(x => x.Sent.Contains("notes")).WithMessage("notes must not be null.");
    }
}

public class UpdateCatRequestValidator : AbstractValidator<UpdateCatRequest>
{
    public UpdateCatRequestValidator()
    {
        Include(new BaseUpdateRequestValidator());
        RuleFor(x => x.Name).NotNull().When(x => x.Sent.Contains("name")).WithMessage("name must not be null.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) >= 1).WithMessage("name must be at least 1 character long.");
        RuleFor(x => x.Name).Must(value => value == null || Fields.Length(value) <= 100).WithMessage("name must be at most 100 characters long.");
        RuleFor(x => x.AgeYears).NotNull().When(x => x.Sent.Contains("ageYears")).WithMessage("ageYears must not be null.");
        RuleFor(x => x.AgeYears).Must(Fields.IsSafeInteger).WithMessage("ageYears must be an integer between -9007199254740991 and 9007199254740991.");
        RuleFor(x => x.AgeYears).GreaterThanOrEqualTo(0L).WithMessage("ageYears must be at least 0.");
        RuleFor(x => x.AgeYears).LessThanOrEqualTo(40L).WithMessage("ageYears must be at most 40.");
        RuleFor(x => x.WeightKg).GreaterThan(0d).WithMessage("weightKg must be greater than 0.");
        RuleFor(x => x.WeightKg).LessThan(100d).WithMessage("weightKg must be less than 100.");
        RuleFor(x => x.Indoor).NotNull().When(x => x.Sent.Contains("indoor")).WithMessage("indoor must not be null.");
        RuleFor(x => x.OwnerEmail).NotNull().When(x => x.Sent.Contains("ownerEmail")).WithMessage("ownerEmail must not be null.");
        RuleFor(x => x.OwnerEmail).Must(value => value == null || Fields.Matches(value, @"^[^@ \t\n]+@[^@ \t\n]+\.[^@ \t\n]+\z")).WithMessage("ownerEmail must be an email address.");
        RuleFor(x => x.Website).Must(value => value == null || Fields.Matches(value, @"^[A-Za-z][A-Za-z0-9+.-]*:[^ \t\n]+\z")).WithMessage("website must be an absolute URI.");
        RuleFor(x => x.Tags).NotNull().When(x => x.Sent.Contains("tags")).WithMessage("tags must not be null.");
        RuleFor(x => x.Tags).Must(values => values == null || values.Count <= 3).WithMessage("tags must have at most 3 items.");
        RuleFor(x => x.Tags).Must(values => values == null || values.Distinct().Count() == values.Count).WithMessage("tags must not repeat an item.");
        RuleFor(x => x.AdoptedAt).Must(Fields.IsDateTime).WithMessage("adoptedAt must be a date-time with an offset, such as 2026-01-31T09:30:00Z.");
        RuleFor(x => x.Notes).NotNull().When(x => x.Sent.Contains("notes")).WithMessage("notes must not be null.");
    }
}
