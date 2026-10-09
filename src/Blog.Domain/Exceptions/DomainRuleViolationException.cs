namespace Blog.Domain.Exceptions;

public class DomainRuleViolationException : DomainException
{
    public DomainRuleViolationException(string message) : base(message)
    {
    }

    public DomainRuleViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
