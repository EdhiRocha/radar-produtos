using System.Net;
using System.Text.Json;
using RadarProdutos.Application.DTOs;
using RadarProdutos.Application.Exceptions;

namespace RadarProdutos.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ConfigurationNotFoundException configEx => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = ApiResponse.ErrorResponse(
                    "Configuração não encontrada",
                    new List<string> { configEx.Message }
                )
            },
            ExternalApiException apiEx => new
            {
                StatusCode = apiEx.StatusCode ?? (int)HttpStatusCode.BadGateway,
                Response = ApiResponse.ErrorResponse(
                    $"Erro ao comunicar com {apiEx.ApiName}",
                    new List<string> { apiEx.Message }
                )
            },
            ValidationException validationEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.ErrorResponse(
                    validationEx.Message,
                    validationEx.Errors
                )
            },
            BusinessException businessEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.ErrorResponse(
                    businessEx.Message,
                    businessEx.Errors.Any() ? businessEx.Errors : null
                )
            },
            HttpRequestException httpEx => new
            {
                StatusCode = (int)HttpStatusCode.BadGateway,
                Response = ApiResponse.ErrorResponse(
                    "Erro ao comunicar com serviço externo",
                    new List<string> { httpEx.Message }
                )
            },
            InvalidOperationException invalidOpEx => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = ApiResponse.ErrorResponse(
                    "Operação inválida",
                    new List<string> { invalidOpEx.Message }
                )
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = ApiResponse.ErrorResponse(
                    "Erro interno do servidor",
                    new List<string> { "Ocorreu um erro inesperado. Por favor, tente novamente." }
                )
            }
        };

        // Log estruturado
        _logger.LogError(exception,
            "Erro capturado: {ExceptionType} - {Message}",
            exception.GetType().Name,
            exception.Message);

        context.Response.StatusCode = response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response.Response, jsonOptions)
        );
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
