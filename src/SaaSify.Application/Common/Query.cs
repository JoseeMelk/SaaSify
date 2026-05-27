using MediatR;

namespace SaaSify.Application.Common;

/// <summary>
/// Clase base para todas las queries en la aplicación.
/// Una query representa una lectura que NO cambia estado.
/// 
/// Ejemplo: GetUserByIdQuery, ListCustomersQuery, CheckEntitlementQuery
/// 
/// Genérico en TResponse para devolver el resultado de la consulta.
/// Las queries nunca generan side effects.
/// </summary>
public abstract class Query<TResponse> : IRequest<TResponse>
{
    // MediatR detecta automáticamente los handlers para esta query
}