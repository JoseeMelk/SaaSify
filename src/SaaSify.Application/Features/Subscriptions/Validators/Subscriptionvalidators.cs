using FluentValidation;
using SaaSify.Application.Features.Subscriptions.Commands;

namespace SaaSify.Application.Features.Subscriptions.Validators;

public class AssignPlanCommandValidator : AbstractValidator<AssignPlanCommand>
{
    public AssignPlanCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");
        RuleFor(x => x.ProjectId).NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");
        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("ExternalId is required")
            .MaximumLength(255).WithMessage("ExternalId must not exceed 255 characters")
            .Matches(@"^\S+$").WithMessage("ExternalId must not contain whitespace");
        RuleFor(x => x.PlanSlug)
            .NotEmpty().WithMessage("PlanSlug is required")
            .MaximumLength(100).WithMessage("PlanSlug must not exceed 100 characters");
        RuleFor(x => x.BillingCycle)
            .NotEmpty().WithMessage("BillingCycle is required")
            .Must(x => x == "Monthly" || x == "Yearly")
            .WithMessage("BillingCycle must be 'Monthly' or 'Yearly'");
        When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod), () =>
        {
            RuleFor(x => x.PaymentMethod)
                .Must(x => x == "Card" || x == "Transfer" || x == "Cash" || x == "Other")
                .WithMessage("PaymentMethod must be 'Card', 'Transfer', 'Cash' or 'Other'");
        });
    }
}

public class RenewSubscriptionCommandValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");
        RuleFor(x => x.ProjectId).NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");
        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("ExternalId is required")
            .Matches(@"^\S+$").WithMessage("ExternalId must not contain whitespace");
    }
}

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");
        RuleFor(x => x.ProjectId).NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");
        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("ExternalId is required")
            .Matches(@"^\S+$").WithMessage("ExternalId must not contain whitespace");
    }
}