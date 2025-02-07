namespace Maboroshi.TemplateEngine.TemplateNodes;

internal class LiteralNode(string value) : TemplateNode
{
    public string Value { get; } = value;
}
