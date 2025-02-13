namespace Maboroshi.TemplateEngine.TemplateNodes;

internal abstract class TemplateNode
{
    public abstract T Accept<T>(ITemplateNodeVisitor<T> visitor);
    public bool IsValid { get; }
}
