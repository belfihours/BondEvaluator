namespace BondEvaluator.Application.Exceptions;

public class BondParserException : Exception
{
    public BondParserException()
    {

    }

    public BondParserException(string message) : base(message)
    {

    }

    public BondParserException(string message, Exception innerException) : base(message, innerException)
    {

    }
}