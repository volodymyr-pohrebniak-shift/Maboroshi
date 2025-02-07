namespace Maboroshi.TemplateEngine.TemplateNodes;

internal class VariableNode(string value) : TemplateNode
{
    public string Value { get; } = value;
}
