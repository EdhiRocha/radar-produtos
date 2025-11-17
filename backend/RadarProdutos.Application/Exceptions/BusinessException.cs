namespace RadarProdutos.Application.Exceptions;

public class BusinessException : Exception
{
    public List<string> Errors { get; }

    public BusinessException(string message) : base(message)
    {
        Errors = new List<string>();
    }

    public BusinessException(string message, List<string> errors) : base(message)
    {
        Errors = errors;
    }

    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = new List<string>();
    }
}

public class ConfigurationNotFoundException : BusinessException
{
    public ConfigurationNotFoundException(string configName)
        : base($"{configName} não encontrado no banco de dados. Execute as migrations.")
    {
    }
}

public class ExternalApiException : BusinessException
{
    public int? StatusCode { get; }
    public string? ApiName { get; }

    public ExternalApiException(string apiName, string message, int? statusCode = null)
        : base($"Erro na API {apiName}: {message}")
    {
        ApiName = apiName;
        StatusCode = statusCode;
    }

    public ExternalApiException(string apiName, string message, Exception innerException)
        : base($"Erro na API {apiName}: {message}", innerException)
    {
        ApiName = apiName;
    }
}

public class ValidationException : BusinessException
{
    public ValidationException(string message, List<string> errors)
        : base(message, errors)
    {
    }
}
