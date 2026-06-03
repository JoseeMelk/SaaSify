using MediatR;

namespace SaaSify.Application.Common;

/// <summary>
/// Clase base para todos los commands en la aplicación.
/// Un command representa una acción que cambia estado.
/// 
/// Ejemplo: RegisterUserCommand, CreateProjectCommand, AssignPlanCommand
/// 
/// Genérico en TResponse para devolver el resultado de la operación.
/// </summary>
public abstract class Command<TResponse> : IRequest<TResponse>
{
    // MediatR detecta automáticamente los handlers para este command
}

/// <summary>
/// Para commands que no devuelven nada (solo confirman ejecución).
/// En la práctica, casi siempre se devuelve algo (el objeto creado, por ejemplo).
/// </summary>
public abstract class Command : IRequest
{
}