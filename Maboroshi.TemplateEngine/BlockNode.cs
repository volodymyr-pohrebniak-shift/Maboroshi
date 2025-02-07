using Maboroshi.TemplateEngine.TemplateNodes;

namespace Maboroshi.TemplateEngine;

internal class BlockNode(string name, List<TemplateNode> parameters, List<TemplateNode> nodes) : TemplateNode
{
    public string Name { get; } = name;
    public List<TemplateNode> Parameters { get; } = parameters;
    public List<TemplateNode> Body { get; } = nodes;
}
