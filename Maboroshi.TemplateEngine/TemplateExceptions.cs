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

// a resource guard rather than a template mistake, so unlike TemplateRuntimeException it fails rendering even in non-strict mode
public class TemplateLimitExceededException : TemplateException
{
    public TemplateLimitExceededException(string message) : base(message)
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
