using FluentValidation;
using SaaSify.Application.Features.Customers.Commands;

namespace SaaSify.Application.Features.Customers.Validators;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");
        RuleFor(x => x.ProjectId).NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");
        RuleFor(x => x.ExternalId).NotEmpty().WithMessage("ExternalId is required")
            .MaximumLength(255).WithMessage("ExternalId must not exceed 255 characters")
            .Matches(@"^\S+$").WithMessage("ExternalId must not contain whitespace"); // Sin espacios
        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name).MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        });
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email).EmailAddress().WithMessage("Email is not valid")
                .MaximumLength(320).WithMessage("Email must not exceed 320 characters");
        });
        RuleFor(x => x.PlanSlug).NotEmpty().WithMessage("PlanSlug is required")
            .MaximumLength(100).WithMessage("PlanSlug must not exceed 100 characters");
    }
}