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
        var tokens = Tokenize("{{ /end }}");

        tokens.Should().HaveCount(4);
        tokens[1].TokenType.Should().Be(TokenType.BLOCK_END);
        tokens[1].Value.Should().Be("end");
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

    [Fact]
    public void Tokenize_ShouldFail_WhenBlockIsClosedWithWrongKeyword()
    {
        var act = () => Tokenize("{{ /notend }}");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Tokenize_ShouldTreatClosingBracesOutsideExpressionAsText()
    {
        var tokens = Tokenize("{\"a\":{\"b\":1}}");

        tokens.Should().HaveCount(2);
        tokens[0].TokenType.Should().Be(TokenType.TEXT);
        tokens[0].Value.Should().Be("{\"a\":{\"b\":1}}");
    }

    [Fact]
    public void Tokenize_ShouldCloseSubExpression_AfterNumberAndFunctionName()
    {
        var tokens = Tokenize("{{ add (add 2 3) (firstname) }}");

        tokens.Select(t => t.TokenType).Should().Equal(
            TokenType.EXPRESSION_START, TokenType.FUNCTION_NAME,
            TokenType.SUB_EXP_START, TokenType.FUNCTION_NAME, TokenType.NUMBER, TokenType.NUMBER, TokenType.SUB_EXP_END,
            TokenType.SUB_EXP_START, TokenType.FUNCTION_NAME, TokenType.SUB_EXP_END,
            TokenType.EXPRESSION_END, TokenType.EOF);
    }

    [Fact]
    public void Tokenize_ShouldParseDecimalAndNegativeNumbers()
    {
        var tokens = Tokenize("{{ add 2.5 -1 }}");

        tokens[2].Should().Match<Token>(t => t.TokenType == TokenType.NUMBER && t.Value == "2.5");
        tokens[3].Should().Match<Token>(t => t.TokenType == TokenType.NUMBER && t.Value == "-1");
    }

    [Fact]
    public void Tokenize_ShouldFail_WhenNumberIsInvalid()
    {
        var act = () => Tokenize("{{ 1.2.3 }}");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Tokenize_ShouldKeepWhitespace_AroundInlineExpressions()
    {
        var tokens = Tokenize("Hello {{ @name }}!\n");

        tokens[0].Value.Should().Be("Hello ");
        tokens[^2].Value.Should().Be("!\n");
    }

    [Fact]
    public void Tokenize_ShouldRemoveLines_WithStandaloneBlockTags()
    {
        var tokens = Tokenize("a\n  {{#if @x}}\n  b\n  {{/end}}\nc");

        tokens.Where(t => t.TokenType == TokenType.TEXT).Select(t => t.Value)
              .Should().Equal("a\n", "  b\n", "c");
    }

    [Fact]
    public void Tokenize_ShouldNotRemoveLine_WhenBlockTagSharesItWithText()
    {
        var tokens = Tokenize("a {{#if @x}}b{{/end}}\n");

        tokens.Where(t => t.TokenType == TokenType.TEXT).Select(t => t.Value)
              .Should().Equal("a ", "b", "\n");
    }

    private static List<Token> Tokenize(string template)
    {
        var lexer = new Lexer(template);
        return lexer.Tokenize().ToList();
    }
}