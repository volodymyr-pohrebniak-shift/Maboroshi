namespace Maboroshi.TemplateEngine;

public class TemplateEngine
{
    public static Template CreateTemplate(string templateStr)
    {


        return new Template(templateStr);
    }

}

public record TemplateCompilationOptions(bool Strict);