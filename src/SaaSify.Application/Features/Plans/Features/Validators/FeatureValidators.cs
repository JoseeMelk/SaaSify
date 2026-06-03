using FluentValidation;
using SaaSify.Application.Features.Plans.Features.Commands;

namespace SaaSify.Application.Features.Plans.Features.Validators;

public class CreateFeatureCommandValidator : AbstractValidator<CreateFeatureCommand>
{
    public CreateFeatureCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");

        RuleFor(x => x.ProjectId)
            .NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");

        RuleFor(x => x.PlanId)
            .NotEqual(Guid.Empty).WithMessage("PlanId is invalid");

        // El slug lo define el developer — export_csv, advanced_analytics, etc.
        // Solo letras minúsculas, números y guiones
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Feature slug is required")
            .MaximumLength(100).WithMessage("Feature slug must not exceed 100 characters")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Feature slug must contain only lowercase letters, numbers, and hyphens");
    }
}

public class RemoveFeatureCommandValidator : AbstractValidator<RemoveFeatureCommand>
{
    public RemoveFeatureCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");

        RuleFor(x => x.ProjectId)
            .NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");

        RuleFor(x => x.PlanId)
            .NotEqual(Guid.Empty).WithMessage("PlanId is invalid");

        RuleFor(x => x.FeatureId)
            .NotEqual(Guid.Empty).WithMessage("FeatureId is invalid");
    }
}