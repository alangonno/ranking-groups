namespace backend.src.Common.Exceptions;

public class BusinessRuleException : Exception
{
    public string Rule { get; }

    public BusinessRuleException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }
}
