namespace Maboroshi.TemplateEngine.TemplateNodes;

internal sealed class TextNode(string value) : TemplateNode
{
    public string Value { get; } = value;

    public override T Accept<T>(ITemplateNodeVisitor<T> visitor)
    {
        return visitor.VisitTextNode(this);
    }
}
