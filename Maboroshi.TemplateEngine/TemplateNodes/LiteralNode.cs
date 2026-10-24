namespace Maboroshi.TemplateEngine.TemplateNodes;

internal sealed class LiteralNode(string value, bool isNumber = false) : TemplateNode
{
    public string Value { get; } = value;
    public bool IsNumber { get; } = isNumber;

    public override T Accept<T>(ITemplateNodeVisitor<T> visitor)
    {
        return visitor.VisitLiteralNode(this);
    }
}
