namespace Maboroshi.TemplateEngine.TemplateNodes;

internal sealed class BlockNode(string name, List<TemplateNode> parameters, List<TemplateNode> nodes) : TemplateNode
{
    public string Name { get; } = name;
    public List<TemplateNode> Parameters { get; } = parameters;
    public List<TemplateNode> Body { get; } = nodes;

    public override T Accept<T>(ITemplateNodeVisitor<T> visitor)
    {
        return visitor.VisitBlockNode(this);
    }
}
