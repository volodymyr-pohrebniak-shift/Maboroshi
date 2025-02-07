namespace Maboroshi.TemplateEngine;

internal class Token(TokenType tokenType, string value)
{
    public TokenType TokenType { get; } = tokenType;
    public string Value { get; } = value;

    public override string ToString() => $"{TokenType}: {Value}";
}

internal enum TokenType
{

    EXPRESSION_START, EXPRESSION_END,
    SUB_EXP_START, SUB_EXP_END,
    BLOCK_START, BLOCK_END,
    
    // Literals
    FUNCTION_NAME, STRING, NUMBER, VAR_IDENTIFIER,

    // Everything outside of expressions
    TEXT,

    EOF
}
