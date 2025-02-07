using Maboroshi.TemplateEngine.TemplateNodes;

namespace Maboroshi.TemplateEngine;

public class TemplateParsingException(string message) : Exception(message);

internal class ExpressionNode(List<TemplateNode> nodes) : TemplateNode
{
    public List<TemplateNode> Parameters { get; } = nodes;
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
        var current = _tokens[_current++];

        if (current.TokenType == endTokenType) return null;

        TemplateNode? node = current.TokenType switch
        {
            TokenType.BLOCK_START => ParseBlock(),
            TokenType.FUNCTION_NAME => ParseFunction(endTokenType),
            TokenType.STRING => new LiteralNode(current.Value),
            TokenType.NUMBER => new LiteralNode(current.Value),
            TokenType.VAR_IDENTIFIER => new VariableNode(current.Value),
            TokenType.SUB_EXP_START => ParseExpression(TokenType.EXPRESSION_END),
            _ => throw new TemplateParsingException($"Unexpected token: {current.TokenType}")
        };

        Consume(endTokenType);

        return node;
    }

    private FunctionNode ParseFunction(TokenType endTokenType = TokenType.EXPRESSION_END)
    {
        var functionName = Current.Value;

        var parameters = new List<TemplateNode>();
        while (Current.TokenType != endTokenType && !IsAtEnd())
        {
            var node = ParseExpression(endTokenType);
            if (node != null)
                parameters.Add(node);
        }

        return new FunctionNode(functionName, parameters);
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
