namespace Maboroshi.TemplateEngine;

public class TemplateParsingException(string message) : Exception(message);

public abstract class TemplateNode
{

}

public class TextNode(string value) : TemplateNode
{
    public string Value { get; } = value;
}

public class ExpressionNode(List<TemplateNode> nodes) : TemplateNode
{
    public List<TemplateNode> Parameters { get; } = nodes;
}

public class BlockNode(string name, List<TemplateNode> parameters, List<TemplateNode> nodes) : TemplateNode
{
    public string Name { get; } = name;
    public List<TemplateNode> Parameters { get; } = parameters;
    public List<TemplateNode> Body { get; } = nodes;
}

public class LiteralNode(string value) : TemplateNode
{
    public string Value { get; } = value;
}

public class VariableNode(string value) : TemplateNode
{
    public string Value { get; } = value;
}

public class IdentifierNode(string value) : TemplateNode
{
    public string Value { get; } = value;
}

internal class Parser(List<Token> tokens)
{
    private readonly List<Token> _tokens = tokens;
    private int _current;
    private Token Current => _tokens[_current];
    private Token Previous => _tokens[_current - 1];

    public List<TemplateNode> Parse()
    {
        if (_tokens.Count == 0)
            return [];

        var result = new List<TemplateNode>();

        while (!IsAtEnd())
        {
            var node = ParseNode();
            if (node != null)
            {
                result.Add(node);
            }
        }
        
        return result;
    }

    private TemplateNode? ParseNode()
    {
        if (Match(TokenType.TEXT))
        {
            return new TextNode(Previous.Value);
        }

        if (Match(TokenType.EXPRESSION_START))
        {
            return ParseExpression();
        }

        return null;
    }

    private TemplateNode? ParseExpression(TokenType endTokenType = TokenType.EXPRESSION_END)
    {
        if (Current.TokenType == TokenType.BLOCK_START)
        {
            return ParseBlock();
        }

        var nodes = new List<TemplateNode>();

        while (Current.TokenType != endTokenType && !IsAtEnd())
        {
            if (Match(TokenType.SUB_EXP_START))
            {
                var node = ParseExpression(TokenType.SUB_EXP_END);
                if (node != null)
                    nodes.Add(node);
            }
            else if (Match(TokenType.STRING))
            {
                nodes.Add(new LiteralNode(Previous.Value));
            }
            else if (Match(TokenType.NUMBER))
            {
                nodes.Add(new LiteralNode(Previous.Value));
            }
            else if (Match(TokenType.VAR_IDENTIFIER))
            {
                nodes.Add(new VariableNode(Previous.Value));
            }
            else if (Match(TokenType.IDENTIFIER))
            {
                nodes.Add(new IdentifierNode(Previous.Value));
            }
            else
            {
                throw new TemplateParsingException($"Unexpected token: {Current.TokenType}");
            }
        }

        Consume(endTokenType);

        if (nodes.Count == 0)
            return null;

        return new ExpressionNode(nodes);
    }

    private BlockNode? ParseBlock()
    {
        var blockName = Current.Value;
        Consume(TokenType.BLOCK_START);

        var parameters = new List<TemplateNode>();
        while (Current.TokenType != TokenType.EXPRESSION_END && !IsAtEnd())
        {
            if (Match(TokenType.SUB_EXP_START))
            {
                var node = ParseExpression(TokenType.SUB_EXP_END);
                if (node != null)
                    parameters.Add(node);
            }
            else if (Match(TokenType.STRING))
            {
                parameters.Add(new LiteralNode(Previous.Value));
            }
            else if (Match(TokenType.NUMBER))
            {
                parameters.Add(new LiteralNode(Previous.Value));
            }
            else if (Match(TokenType.VAR_IDENTIFIER))
            {
                parameters.Add(new VariableNode(Previous.Value));
            }
            else
            {
                throw new TemplateParsingException($"Unexpected token: {Previous.TokenType}");
            }
        }
        Consume(TokenType.EXPRESSION_END);
        var nodes = new List<TemplateNode> ();
        while (!IsAtEnd())
        {
            if (Match(TokenType.TEXT))
            {
                nodes.Add(new TextNode(Previous.Value));
            }
            if (Match(TokenType.EXPRESSION_START)) 
            {
                if (Match(TokenType.BLOCK_END))
                {
                    break;
                } else 
                {
                    var node = ParseExpression();
                    if (node != null)
                        nodes.Add(node);
                }
            }
        }
        Consume(TokenType.EXPRESSION_END);
        return nodes.Count == 0 ? null : new BlockNode(blockName, parameters, nodes);
    }

    private bool Match(params TokenType[] tokenTypes)
    {
        foreach (var tokenType in tokenTypes)
        {
            if (Current.TokenType == tokenType)
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool IsAtEnd()
    {
        return Current.TokenType == TokenType.EOF;
    }

    private bool Check(TokenType type) => !IsAtEnd() && Current.TokenType == type;

    private void Advance()
    {
        if (!IsAtEnd())
        {
            _current++;
        }
    }

    private void Consume(TokenType type)
    {
        if (Check(type))
        {
            Advance();
        }
        else
        {
            throw new TemplateParsingException($"Expected token {type}, but got {Current.TokenType}");
        }
    }
}
