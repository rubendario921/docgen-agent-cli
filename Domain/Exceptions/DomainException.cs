namespace DocGen_Agent.Domain.Exceptions;

/// <summary>
/// Excepción base para reglas de negocio y lógica de dominio.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message, string code = "DOMAIN_ERROR") 
        : base(message)
    {
        Code = code;
    }

    public DomainException(string message, Exception innerException, string code = "DOMAIN_ERROR") 
        : base(message, innerException)
    {
        Code = code;
    }
}
