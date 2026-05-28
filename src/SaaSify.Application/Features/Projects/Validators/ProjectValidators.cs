using FluentValidation;
using SaaSify.Application.Features.Projects.Commands;

namespace SaaSify.Application.Features.Projects.Validators;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.OwnerId).NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");
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
    }
}

public class RotateApiKeyCommandValidator : AbstractValidator<RotateApiKeyCommand>
{
    public RotateApiKeyCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEqual(Guid.Empty).WithMessage("ProjectId is invalid");
        RuleFor(x => x.OwnerId).NotEqual(Guid.Empty).WithMessage("OwnerId is invalid");
    }
}