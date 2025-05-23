﻿namespace Maboroshi.TemplateEngine.TemplateNodes;

internal sealed class FunctionNode(string name, List<TemplateNode> parameters) : TemplateNode
{
    public string Name { get; } = name;
    public List<TemplateNode> Parameters { get; } = parameters;

    public override T Accept<T>(ITemplateNodeVisitor<T> visitor)
    {
        return visitor.VisitFunctionNode(this);
    }
}
