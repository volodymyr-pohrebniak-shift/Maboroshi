using Maboroshi.TemplateEngine.TemplateNodes;

namespace Maboroshi.TemplateEngine;

internal interface ITemplateNodeVisitor<T>
{
    T VisitTextNode(TextNode node);
    T VisitLiteralNode(LiteralNode node);
    T VisitVariableNode(VariableNode node);
    T VisitFunctionNode(FunctionNode node);
    T VisitBlockNode(BlockNode node);
}
