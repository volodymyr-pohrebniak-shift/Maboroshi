namespace Maboroshi.TemplateEngine;

public class TemplateException : Exception
{
    public TemplateException(string message) : base(message)
    {
    }

    public TemplateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class TemplateParsingException : TemplateException
{
    public TemplateParsingException(string message) : base(message)
    {
    }

    public TemplateParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class TemplateRuntimeException : TemplateException
{
    public TemplateRuntimeException(string message) : base(message)
    {
    }

    public TemplateRuntimeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
