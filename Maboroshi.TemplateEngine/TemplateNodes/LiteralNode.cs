namespace Maboroshi.TemplateEngine.TemplateNodes;

internal sealed class LiteralNode(string value) : TemplateNode
{
    public string Value { get; } = value;

    public override T Accept<T>(ITemplateNodeVisitor<T> visitor)
    {
        return visitor.VisitLiteralNode(this);
    }
}
