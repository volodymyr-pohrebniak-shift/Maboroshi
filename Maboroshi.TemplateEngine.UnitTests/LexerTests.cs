using FluentAssertions;
namespace Maboroshi.TemplateEngine.UnitTests;

public class LexerTests
{
    [Fact]
    public void Tokenize_ShouldReturnEmptyList_ForEmptyString()
    {
        var tokens = Tokenize("");
        tokens.Should().ContainSingle(t => t.TokenType == TokenType.EOF);
    }

    [Fact]
    public void Tokenize_ShouldParseEmptyExpression()
    {
        var tokens = Tokenize("{{ }}");

        tokens.Should().HaveCount(3);
        tokens[0].TokenType.Should().Be(TokenType.EXPRESSION_START);
        tokens[1].TokenType.Should().Be(TokenType.EXPRESSION_END);
        tokens[2].TokenType.Should().Be(TokenType.EOF);
    }

    [Fact]
    public void Tokenize_ShouldParseText()
    {
        var tokens = Tokenize("Hello, world!");

        tokens.Should().HaveCount(2);
        tokens[0].TokenType.Should().Be(TokenType.TEXT);
        tokens[0].Value.Should().Be("Hello, world!");
        tokens[1].TokenType.Should().Be(TokenType.EOF);
    }

    [Fact]
    public void Tokenize_ShouldParseLiteral()
    {
        var tokens = Tokenize("{{ 'hello' }}");

        tokens.Should().HaveCount(4);
        tokens[1].TokenType.Should().Be(TokenType.STRING);
        tokens[1].Value.Should().Be("hello");
    }

    [Fact]
    public void Tokenize_ShouldParseVariable()
    {
        var tokens = Tokenize("{{ @user }}");

        tokens.Should().HaveCount(4);
        tokens[1].TokenType.Should().Be(TokenType.VAR_IDENTIFIER);
        tokens[1].Value.Should().Be("user");
    }

    [Fact]
    public void Tokenize_ShouldParseFunctionWithParameter()
    {
        var tokens = Tokenize("{{ uppercase 'hello' }}");

        tokens.Should().HaveCount(5);
        tokens[1].TokenType.Should().Be(TokenType.FUNCTION_NAME);
        tokens[1].Value.Should().Be("uppercase");
        tokens[2].TokenType.Should().Be(TokenType.STRING);
        tokens[2].Value.Should().Be("hello");
    }

    [Fact]
    public void Tokenize_ShouldParseBlockStart()
    {
        var tokens = Tokenize("{{ #repeat }}");

        tokens.Should().HaveCount(4);
        tokens[1].TokenType.Should().Be(TokenType.BLOCK_START);
        tokens[1].Value.Should().Be("repeat");
    }

    [Fact]
    public void Tokenize_ShouldParseBlockEnd()
    {
        var tokens = Tokenize("{{ /repeat }}");

        tokens.Should().HaveCount(4);
        tokens[1].TokenType.Should().Be(TokenType.BLOCK_END);
        tokens[1].Value.Should().Be("repeat");
    }

    [Fact]
    public void Tokenize_ShouldParseSubExpression()
    {
        var tokens = Tokenize("{{ (concat 'a' 'b') }}");

        tokens.Should().HaveCount(8);
        tokens[1].TokenType.Should().Be(TokenType.SUB_EXP_START);
        tokens[5].TokenType.Should().Be(TokenType.SUB_EXP_END);
    }

    [Fact]
    public void Tokenize_ShouldFail_WhenBlockNameIsEmpty()
    {
        var act = () => Tokenize("{{ # }}");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Tokenize_ShouldFail_WhenVariableNameIsEmpty()
    {
        var act = () => Tokenize("{{ @ }}");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Tokenize_ShouldFail_WhenStringLiteralIsNotClosed()
    {
        var act = () => Tokenize("{{ 'hello }}");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Tokenize_ShouldFail_WhenExpressionIsNotClosed()
    {
        var act = () => Tokenize("{{ 'hello'");

        act.Should().Throw<InvalidOperationException>();
    }

    private static List<Token> Tokenize(string template)
    {
        var lexer = new Lexer(template);
        return lexer.Tokenize().ToList();
    }
}