namespace MarketStat.Common.Exceptions;

public class AuthenticationException : DomainException
{
    public AuthenticationException(string message) : base(message)
    {
    }

    public AuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}