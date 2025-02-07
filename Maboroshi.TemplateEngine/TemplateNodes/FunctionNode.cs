namespace Maboroshi.TemplateEngine.TemplateNodes;

internal class FunctionNode(string name, List<TemplateNode> parameters) : TemplateNode
{
    public string Name { get; } = name;
    public List<TemplateNode> Parameters { get; } = parameters;
}
