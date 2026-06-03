using FluentValidation;
using SaaSify.Application.Features.Entitlements.Queries;

namespace SaaSify.Application.Features.Entitlements.Validators;

/// <summary>
/// Validador para CheckEntitlementQuery.
/// 
/// Son validaciones mínimas — este endpoint debe ser
/// lo más rápido posible. Se evita lógica compleja aquí.
/// </summary>
public class CheckEntitlementQueryValidator : AbstractValidator<CheckEntitlementQuery>
{
    public CheckEntitlementQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEqual(Guid.Empty)
            .WithMessage("ProjectId is invalid");

        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("customerId is required")
            .MaximumLength(255).WithMessage("customerId must not exceed 255 characters")
            .Matches(@"^\S+$").WithMessage("customerId must not contain whitespace");

        RuleFor(x => x.Feature)
            .NotEmpty().WithMessage("feature is required")
            .MaximumLength(100).WithMessage("feature must not exceed 100 characters");
    }
}