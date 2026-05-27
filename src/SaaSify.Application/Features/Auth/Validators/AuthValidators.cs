using FluentValidation;
using SaaSify.Application.Features.Auth.Commands;

namespace SaaSify.Application.Features.Auth.Validators;

/// <summary>
/// Validador para RegisterUserCommand.
/// 
/// Se usa FluentValidation para validaciones declarativas.
/// Cada regla es clara y testeable.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Email debe ser válido")
            .MaximumLength(320).WithMessage("Email no puede exceder 320 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerida")
            .MinimumLength(8).WithMessage("Password debe tener al menos 8 caracteres")
            .Must(ContainUpperCase).WithMessage("Password debe contener mayúsculas")
            .Must(ContainLowerCase).WithMessage("Password debe contener minúsculas")
            .Must(ContainDigit).WithMessage("Password debe contener números");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nombre es requerido")
            .MaximumLength(100).WithMessage("Nombre no puede exceder 100 caracteres");
    }

    private static bool ContainUpperCase(string password) =>
        password.Any(char.IsUpper);

    private static bool ContainLowerCase(string password) =>
        password.Any(char.IsLower);

    private static bool ContainDigit(string password) =>
        password.Any(char.IsDigit);
}

/// <summary>
/// Validador para LoginUserCommand.
/// </summary>
public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Email debe ser válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerida");
    }
}

/// <summary>
/// Validador para RefreshTokenCommand.
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token es requerido");
    }
}

/// <summary>
/// Validador para LogoutUserCommand.
/// </summary>
public class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("UserId inválido");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token es requerido");
    }
}