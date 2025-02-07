namespace Maboroshi.TemplateEngine.TemplateNodes;

internal class TextNode(string value) : TemplateNode
{
    public string Value { get; } = value;
}
