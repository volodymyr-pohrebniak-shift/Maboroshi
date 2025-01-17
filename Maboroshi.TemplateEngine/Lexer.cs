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
                _tokens.Add(new Token(TokenType.TEXT, _source[start.._current]));
                _tokens.Add(new Token(TokenType.EXPRESSION_START, "{{"));
                start = _current;
            }
            else if (c == '}' && Peek() == '}')
            {
                expressionStarted = false;
                _tokens.Add(new Token(TokenType.EXPRESSION_END, "}}"));
                start = _current;
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
                    token ??= ReadVariable();
                    token ??= ReadIdentifier();

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
                return new Token(TokenType.VAR_IDENTIFIER, _source[start.._current]);
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
                return new Token(TokenType.STRING, _source[start..(_current+1)]);
            }
        }

        throw new InvalidOperationException("Expression was started, but not closed");
    }

    private Token? ReadIdentifier()
    {
        bool IsValidStartCharacter(char ch)
        {
            return (ch >= '0' && ch <= '9') 
                || (ch >= 'a' && ch <= 'z')
                || (ch >= 'A' && ch <= 'Z')
                || ch == '_';
        }

        if (!IsValidStartCharacter(Peek())) return null;

        var start = _current;

        while (_current < _source.Length)
        {
            Advance();
            if (char.IsWhiteSpace(Peek()) || Peek() == '}')
            {
                return new Token(TokenType.IDENRIFIER, _source[start.._current]);
            }
        }

        throw new InvalidOperationException("Expression was started, but not closed");
    }

    private void ReadText()
    {
        var start = _current;

        while (_current < _source.Length && _source[_current] != '{')
        {
            Advance();
        }

        _tokens.Add(new Token(TokenType.TEXT, _source[start.._current]));
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

    private bool isIdentifierCharacter(char c)
    {
        return (c >= 'a' && c <= 'z') ||
           (c >= 'A' && c <= 'Z') ||
           (c >= '0' && c <= '9') ||
            c == '_';
    }
}
