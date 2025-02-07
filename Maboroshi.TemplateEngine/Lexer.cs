namespace Maboroshi.TemplateEngine;

internal class Lexer(string source)
{
    private readonly string _source = source;
    private readonly List<Token> _tokens = [];
    private int _current = 0;

    public IEnumerable<Token> Tokenize()
    {
        var expressionStarted = false;
        var start = 0;

        while (_current < _source.Length)
        {
            var c = Advance();
            if (c == '{' && Peek() == '{')
            {
                expressionStarted = true;
                _tokens.Add(new Token(TokenType.TEXT, _source[start..(_current - 1)]));
                _tokens.Add(new Token(TokenType.EXPRESSION_START, "{{"));
                start = _current;
            }
            else if (c == '}' && Peek() == '}')
            {
                expressionStarted = false;
                _tokens.Add(new Token(TokenType.EXPRESSION_END, "}}"));
                start = _current + 1;
            }
            else if (expressionStarted)
            {
                if (Peek() == '(')
                {
                    _tokens.Add(new Token(TokenType.SUB_EXP_START, "("));
                } else if (Peek() == ')')
                {
                    _tokens.Add(new Token(TokenType.SUB_EXP_END, ")"));
                } else
                {
                    var token = ReadStringLiteral();
                    token ??= ReadNumber();
                    token ??= ReadVariable();
                    token ??= ReadStartBlock();
                    token ??= ReadEndBlock();
                    token ??= ReadFunctionName();

                    if (token != null)
                    {
                        _tokens.Add(token);
                    }
                }
            }
        }

        if (start != _current)
        {
            if (expressionStarted)
            {
                throw new InvalidOperationException("Expression was started, but not closed");
            } else
            {
                _tokens.Add(new Token(TokenType.TEXT, _source[start.._current]));
            }
        }

        _tokens.Add(new Token(TokenType.EOF, string.Empty));

        return _tokens;
    }

    private Token? ReadVariable()
    {
        if (Peek() != '@') return null;

        var start = _current;
        while (_current < _source.Length)
        {
            Advance();
            if (char.IsWhiteSpace(Peek()) || Peek() == '}') 
            {
                if (_current == start + 1)
                    throw new InvalidOperationException("Variable name can't be empty");
                return new Token(TokenType.VAR_IDENTIFIER, _source[(start+1).._current]);
            }
        }

        throw new InvalidOperationException("Expression was started, but not closed");
    }

    private Token? ReadStringLiteral()
    {
        if (Peek() != '\'') return null;

        var start = _current;

        while (_current < _source.Length)
        {
            Advance();
            if (Peek() == '\'')
            {
                return new Token(TokenType.STRING, _source[(start+1).._current]);
            }
        }

        throw new InvalidOperationException("Expression was started, but not closed");
    }

    private Token? ReadNumber()
    {
        if (!char.IsDigit(Peek())) return null;
        var start = _current;
        
        while (_current < _source.Length)
        {
            Advance();
            if (char.IsWhiteSpace(Peek()))
            {
                return new Token(TokenType.NUMBER, _source[start.._current]);
            }
            if (!char.IsDigit(Peek()))
                throw new InvalidOperationException("Invalid number");
        }

        throw new InvalidOperationException("Expression was started, but not closed");
    }

    private Token? ReadFunctionName()
    {
        static bool IsValidStartCharacter(char ch) => (ch >= '0' && ch <= '9')
                || (ch >= 'a' && ch <= 'z')
                || (ch >= 'A' && ch <= 'Z')
                || ch == '_';

        if (!IsValidStartCharacter(Peek())) return null;

        var start = _current;

        while (_current < _source.Length)
        {
            Advance();
            if (char.IsWhiteSpace(Peek()) || Peek() == '}')
            {
                return new Token(TokenType.FUNCTION_NAME, _source[start.._current]);
            }
        }

        throw new InvalidOperationException("Expression was started, but not closed");
    }

    private Token? ReadStartBlock()
    {
        if (Peek() != '#') return null;

        var start = _current;
        while (_current < _source.Length)
        {
            Advance();
            if (char.IsWhiteSpace(Peek()) || Peek() == '}')
            {
                if (_current == start + 1)
                    throw new InvalidOperationException("Block name can't be empty");
                return new Token(TokenType.BLOCK_START, _source[(start+1).._current]);
            }
        }

        throw new InvalidOperationException("Expression was started, but not closed");
    }

    private Token? ReadEndBlock()
    {
        if (Peek() != '/') return null;

        var start = _current;
        while (_current < _source.Length)
        {
            Advance();
            if (char.IsWhiteSpace(Peek()) || Peek() == '}')
            {
                if (_current == start + 1)
                    throw new InvalidOperationException("Block name can't be empty");
                return new Token(TokenType.BLOCK_END, _source[(start + 1).._current]);
            }
        }

        throw new InvalidOperationException("Expression was started, but not closed");
    }

    private char Advance()
    {
        if (_current >= _source.Length)
        {
            return '\0';
        }

        return _source[_current++];
    }

    private char Peek()
    {
        if (_current >= _source.Length)
        {
            return '\0';
        }

        return _source[_current];
    }
}
