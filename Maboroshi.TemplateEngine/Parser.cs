using Maboroshi.TemplateEngine.TemplateNodes;

namespace Maboroshi.TemplateEngine;

public class TemplateParsingException : Exception
{
    public TemplateParsingException()
    {
    }

    public TemplateParsingException(string message) : base(message)
    {
    }

    public TemplateParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
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
            if (endTokenType == TokenType.SUB_EXP_END)
            {
                throw new TemplateParsingException("Subexpressions can't have blocks");
            }
            return ParseBlock();
        }

        var nodes = new List<TemplateNode>();

        while (Current.TokenType != endTokenType && !IsAtEnd())
        {
            var node = ParseExpressionNode(endTokenType);

            if (node is not null)
                nodes.Add(node);
        }

        Consume(endTokenType);

        return nodes.Count switch
        {
            0 => null,
            1 => nodes[0],
            _ => throw new TemplateParsingException("Expression contains more nodes then expected")
        };
    }

    private TemplateNode? ParseExpressionNode(TokenType endTokenType = TokenType.EXPRESSION_END)
    {
        var current = _tokens[_current++];

        if (current.TokenType == endTokenType) return null;

        return current.TokenType switch
        {
            TokenType.FUNCTION_NAME => ParseFunction(endTokenType),
            TokenType.STRING => new LiteralNode(current.Value),
            TokenType.NUMBER => new LiteralNode(current.Value),
            TokenType.VAR_IDENTIFIER => new VariableNode(current.Value),
            TokenType.SUB_EXP_START => ParseExpression(TokenType.SUB_EXP_END),
            _ => throw new TemplateParsingException($"Unexpected token: {current.TokenType}")
        };
    }

    private FunctionNode ParseFunction(TokenType endTokenType = TokenType.EXPRESSION_END)
    {
        var functionName = Previous.Value;
        var parameters = new List<TemplateNode>();
        while (Current.TokenType != endTokenType && !IsAtEnd())
        {
            var node = ParseExpressionNode(endTokenType);
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
            var node = ParseExpressionNode();

            if (node is not null)
                parameters.Add(node);
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
