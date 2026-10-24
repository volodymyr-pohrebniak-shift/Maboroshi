namespace Maboroshi.TemplateEngine;

internal class Lexer(string source)
{
    // Tags that produce no output; a line holding only such a tag is removed entirely (Handlebars "standalone" rule)
    private static readonly HashSet<string> StandaloneFunctions = ["else", "elsif", "var"];

    private readonly string _source = source;
    private readonly List<Token> _tokens = [];
    private int _current;

    public IEnumerable<Token> Tokenize()
    {
        while (_current < _source.Length)
        {
            var expressionStart = _source.IndexOf("{{", _current, StringComparison.Ordinal);
            if (expressionStart < 0)
            {
                _tokens.Add(new Token(TokenType.TEXT, _source[_current..]));
                break;
            }

            if (expressionStart > _current)
                _tokens.Add(new Token(TokenType.TEXT, _source[_current..expressionStart]));

            _tokens.Add(new Token(TokenType.EXPRESSION_START, "{{"));
            _current = expressionStart + 2;
            TokenizeExpression();
        }

        _tokens.Add(new Token(TokenType.EOF, string.Empty));

        return TrimStandaloneLines(_tokens);
    }

    private void TokenizeExpression()
    {
        while (true)
        {
            while (_current < _source.Length && char.IsWhiteSpace(_source[_current])) _current++;

            if (_current >= _source.Length)
                throw new TemplateParsingException("Expression was started, but not closed");

            var c = _source[_current];
            if (c == '}' && Peek(1) == '}')
            {
                _tokens.Add(new Token(TokenType.EXPRESSION_END, "}}"));
                _current += 2;
                return;
            }

            _tokens.Add(c switch
            {
                '(' => ReadSingleChar(TokenType.SUB_EXP_START),
                ')' => ReadSingleChar(TokenType.SUB_EXP_END),
                '\'' => ReadStringLiteral(),
                '@' => ReadVariable(),
                '#' => ReadStartBlock(),
                '/' => ReadEndBlock(),
                _ when char.IsDigit(c) || (c == '-' && char.IsDigit(Peek(1))) => ReadNumber(),
                _ => ReadFunctionName()
            });
        }
    }

    private Token ReadSingleChar(TokenType tokenType)
    {
        return new Token(tokenType, _source[_current++].ToString());
    }

    private Token ReadStringLiteral()
    {
        var end = _source.IndexOf('\'', _current + 1);
        if (end < 0)
            throw new TemplateParsingException("String literal is not closed");

        var token = new Token(TokenType.STRING, _source[(_current + 1)..end]);
        _current = end + 1;
        return token;
    }

    private Token ReadNumber()
    {
        var start = _current;
        if (_source[_current] == '-') _current++;
        SkipDigits();

        if (Peek(0) == '.' && char.IsDigit(Peek(1)))
        {
            _current++;
            SkipDigits();
        }

        if (_current < _source.Length && !IsTerminator(_source[_current]))
            throw new TemplateParsingException("Invalid number");

        return new Token(TokenType.NUMBER, _source[start.._current]);
    }

    private Token ReadVariable()
    {
        _current++;
        var name = ReadWord();
        if (name.Length == 0)
            throw new TemplateParsingException("Variable name can't be empty");

        return new Token(TokenType.VAR_IDENTIFIER, name);
    }

    private Token ReadStartBlock()
    {
        _current++;
        var name = ReadWord();
        if (name.Length == 0)
            throw new TemplateParsingException("Block name can't be empty");

        return new Token(TokenType.BLOCK_START, name);
    }

    private Token ReadEndBlock()
    {
        _current++;
        var name = ReadWord();
        if (name.Length == 0)
            throw new TemplateParsingException("Block name can't be empty");

        if (name != "end")
            throw new TemplateParsingException("Block should be closed by 'end' keyword");

        return new Token(TokenType.BLOCK_END, name);
    }

    private Token ReadFunctionName()
    {
        var name = ReadWord();
        if (name.Length == 0)
            throw new TemplateParsingException($"Unexpected character '{_source[_current]}'");

        return new Token(TokenType.FUNCTION_NAME, name);
    }

    private string ReadWord()
    {
        var start = _current;
        while (_current < _source.Length && !IsTerminator(_source[_current])) _current++;
        return _source[start.._current];
    }

    private void SkipDigits()
    {
        while (_current < _source.Length && char.IsDigit(_source[_current])) _current++;
    }

    private static bool IsTerminator(char c) => char.IsWhiteSpace(c) || c is '(' or ')' or '}' or '\'';

    private char Peek(int offset)
    {
        var index = _current + offset;
        return index < _source.Length ? _source[index] : '\0';
    }

    private static List<Token> TrimStandaloneLines(List<Token> tokens)
    {
        var trimEnd = new bool[tokens.Count];
        var trimStart = new bool[tokens.Count];

        // decisions are made on the original text, so that tags on adjacent lines don't affect each other
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].TokenType != TokenType.EXPRESSION_START)
                continue;

            var end = tokens.FindIndex(i, t => t.TokenType == TokenType.EXPRESSION_END);
            if (!IsStandaloneTag(tokens[i + 1]) || !StartsLine(tokens, i) || !EndsLine(tokens, end))
                continue;

            if (i > 0) trimEnd[i - 1] = true;
            trimStart[end + 1] = true;
        }

        var result = new List<Token>(tokens.Count);
        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].TokenType != TokenType.TEXT || (!trimEnd[i] && !trimStart[i]))
            {
                result.Add(tokens[i]);
                continue;
            }

            var text = tokens[i].Value;
            if (trimEnd[i])
                text = text.TrimEnd(' ', '\t');
            if (trimStart[i])
                text = TrimFirstLineBreak(text);

            if (text.Length > 0)
                result.Add(new Token(TokenType.TEXT, text));
        }

        return result;
    }

    private static bool IsStandaloneTag(Token firstExpressionToken) => firstExpressionToken.TokenType switch
    {
        TokenType.BLOCK_START or TokenType.BLOCK_END => true,
        TokenType.FUNCTION_NAME => StandaloneFunctions.Contains(firstExpressionToken.Value),
        _ => false
    };

    private static bool StartsLine(List<Token> tokens, int expressionStart)
    {
        if (expressionStart == 0)
            return true;

        var previous = tokens[expressionStart - 1];
        if (previous.TokenType != TokenType.TEXT)
            return false;

        var lineBreak = previous.Value.LastIndexOf('\n');
        return IsBlank(previous.Value.AsSpan(lineBreak + 1)) && (lineBreak >= 0 || expressionStart == 1);
    }

    private static bool EndsLine(List<Token> tokens, int expressionEnd)
    {
        var next = tokens[expressionEnd + 1];
        if (next.TokenType == TokenType.EOF)
            return true;

        if (next.TokenType != TokenType.TEXT)
            return false;

        var lineBreak = next.Value.IndexOf('\n');
        return lineBreak >= 0
            ? IsBlank(next.Value.AsSpan(0, lineBreak))
            : IsBlank(next.Value) && tokens[expressionEnd + 2].TokenType == TokenType.EOF;
    }

    private static bool IsBlank(ReadOnlySpan<char> text)
    {
        foreach (var c in text)
        {
            if (c is not (' ' or '\t' or '\r'))
                return false;
        }

        return true;
    }

    private static string TrimFirstLineBreak(string text)
    {
        var i = 0;
        while (i < text.Length && text[i] is ' ' or '\t') i++;
        if (i < text.Length && text[i] == '\r') i++;
        if (i < text.Length && text[i] == '\n') i++;
        return text[i..];
    }
}
