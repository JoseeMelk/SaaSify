using FluentValidation;
using SaaSify.Application.Features.Plans.Commands;

namespace SaaSify.Application.Features.Plans.Validators;

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");
        RuleFor(X => X.ProjectId).NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Project name is required").MaximumLength(100).WithMessage("Project name must not exceed 100 characters");
        // El slug es opcional - si viene, se valida su formato.
        // Solo letras minusculas, numeros y guiones - sin espacios ni caracteres especiales
        // Ejemplo mi-saas-app o acme-corp-v2
        When(x => !string.IsNullOrWhiteSpace(x.Slug), () =>
        {
            RuleFor(x => x.Slug)
                .MaximumLength(100).WithMessage("Slug must not exceed 100 characters")
                .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");
        });
        // Mayor o igual a cero
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be a positive value");
        // Currency requerido si Price tiene valor y es mayor a 0
        When(x => x.Price.HasValue && x.Price > 0, () =>
        {
            RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required when price is specified");
        });
        // BillingCycle — si viene, solo puede ser "Monthly" o "Yearly"
        When(x => !string.IsNullOrWhiteSpace(x.BillingCycle), () =>
        {
            RuleFor(x => x.BillingCycle).Must(x => x == "Monthly" || x == "Yearly").WithMessage("BillingCycle must be 'Monthly' or 'Yearly'");
        });
    }
}

public class DeactivatePlanCommandValidator : AbstractValidator<DeactivatePlanCommand>
{
    public DeactivatePlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty).WithMessage("PlanId is invalid");
        RuleFor(x => x.OwnerId).NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");
        RuleFor(x => x.ProjectId).NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");
    }
}