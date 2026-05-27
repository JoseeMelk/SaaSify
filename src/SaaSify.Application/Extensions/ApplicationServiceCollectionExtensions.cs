using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SaaSify.Application.Features.Auth.Validators;

namespace SaaSify.Application.Extensions;

/// <summary>
/// Interfaz marcadora para identificar el assembly de Application.
/// Se usa con MediatR para que detecte automáticamente handlers y validadores.
/// No tiene métodos — es solo un marcador de tipo.
/// </summary>
public interface IApplicationMarker
{
}

/// <summary>
/// Extensión de IServiceCollection para registrar todos los servicios del Application Layer.
/// 
/// Se llama desde Program.cs:
///   services.AddApplicationServices();
/// 
/// Esto agrupa todo el setup en un lugar, manteniendo Program.cs limpio.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ── Registrar MediatR ──────────────────────────────────────────────
        // Se le dice a MediatR que busque handlers en este assembly.
        // MediatR detecta automáticamente:
        //   - IRequestHandler<TRequest, TResponse>
        //   - INotificationHandler<TNotification>
        //   - Validators
        // Se usa typeof(IApplicationMarker) como marcador de assembly (no puede ser clase estática).
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<IApplicationMarker>();
        });

        // ── Registrar FluentValidation ────────────────────────────────────
        // Se registran todos los validadores en el assembly actual.
        // MediatR los ejecutará automáticamente antes de cada handler.
        services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();

        // ── Registrar el pipeline de validación en MediatR ────────────────
        // Esto hace que MediatR ejecute FluentValidation antes de los handlers.
        // Si hay errores de validación, nunca llega al handler.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

/// <summary>
/// Pipeline behavior personalizado para MediatR.
/// Se ejecuta ANTES de cada handler y valida automáticamente.
/// 
/// Patrón: Decorator pattern. MediatR lo invoca automáticamente.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Se obtienen todos los validadores registrados para este request.
        var validationContext = new ValidationContext<TRequest>(request);

        // Se ejecutan TODOS los validadores (puede haber múltiples).
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        // Se agregan todos los errores en una sola lista.
        var errors = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        // Si hay errores, se devuelve un error sin llamar al handler.
        if (errors.Any())
        {
            // Se construye un error message de validación.
            var errorMessage = string.Join("; ", errors.Select(e => e.ErrorMessage));
            
            // Se intenta crear un Result.Failure si el response type lo permite.
            // Si TResponse es Result, se devuelve un resultado de error.
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition().Name.StartsWith("Result"))
            {
                var resultType = typeof(TResponse);
                var failureMethod = resultType.GetMethod("Failure");
                
                if (failureMethod != null)
                {
                    var failureResult = failureMethod.Invoke(null, new object?[] { errorMessage, null });
                    return (TResponse)failureResult!;
                }
            }

            // Fallback: lanzar ValidationException (será atrapada por middleware global).
            throw new ValidationException(errors);
        }

        // Si no hay errores, se continúa al siguiente paso (el handler).
        return await next();
    }
}