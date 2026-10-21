using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NexusFlow.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            BusinessRuleException =>
                (StatusCodes.Status400BadRequest, "Solicitud inválida"),

            ValidationException =>
                (StatusCodes.Status400BadRequest, "Datos inválidos"),

            UnauthorizedException =>
                (StatusCodes.Status401Unauthorized, "No autorizado"),

            ForbiddenException =>
                (StatusCodes.Status403Forbidden, "Acceso denegado"),

            NotFoundException =>
                (StatusCodes.Status404NotFound, "Recurso no encontrado"),

            ConflictException =>
                (StatusCodes.Status409Conflict, "Conflicto"),

            _ =>
                (StatusCodes.Status500InternalServerError, "Error interno")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Se produjo un error no controlado al procesar {Path}",
                httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        var written = await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });

        if (!written)
        {
            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);
        }

        return true;
    }
}