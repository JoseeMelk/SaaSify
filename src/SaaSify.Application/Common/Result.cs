namespace SaaSify.Application.Common;

/// <summary>
/// Patrón Result — encapsula éxito o fracaso de una operación.
/// 
/// En vez de lanzar excepciones para "errores de negocio", se devuelve Result
/// que puede estar en estado exitoso o fallido.
/// 
/// Ejemplo:
///   var result = await mediator.Send(command);
///   if (!result.IsSuccess)
///       return BadRequest(result.Error);
/// </summary>
public class Result
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public int? ErrorCode { get; set; }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string error, int? errorCode = null) => new() { IsSuccess = false, Error = error, ErrorCode = errorCode };
}

/// <summary>
/// Result genérico — se devuelve un valor además de éxito/fracaso.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public int? ErrorCode { get; set; }

    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };

    public static Result<T> Failure(string error, int? errorCode = null) => new() { IsSuccess = false, Error = error, ErrorCode = errorCode };
}