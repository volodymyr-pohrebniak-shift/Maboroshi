namespace Maboroshi.TemplateEngine.TemplateNodes;

internal sealed class VariableNode(string value) : TemplateNode
{
    public string Value { get; } = value;
    public override T Accept<T>(ITemplateNodeVisitor<T> visitor)
    {
        return visitor.VisitVariableNode(this);
    }
}
