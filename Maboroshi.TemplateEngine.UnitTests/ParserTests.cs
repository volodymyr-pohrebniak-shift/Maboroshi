using FluentAssertions;
using Maboroshi.TemplateEngine.TemplateNodes;

namespace Maboroshi.TemplateEngine.UnitTests;

public class ParserTests
{
    [Fact]
    public void Parse_ShouldReturnEmptyList_ForEmptyExpression()
    {
        var parser = CreateParser("{{ }}");
        var result = parser.Parse();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldReturnEmptyList_ForEmptySubExpression()
    {
        var parser = CreateParser("{{ () }}");
        var result = parser.Parse();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldParse_SingleLiteral()
    {
        var parser = CreateParser("{{ 'hello' }}");
        var result = parser.Parse();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<LiteralNode>()
                 .Which.Value.Should().Be("hello");
    }

    [Fact]
    public void Parse_ShouldParse_SingleVariable()
    {
        var parser = CreateParser("{{ @variable }}");
        var result = parser.Parse();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<VariableNode>()
                 .Which.Value.Should().Be("variable");
    }

    [Fact]
    public void Parse_ShouldParse_MultipleExpressions()
    {
        var parser = CreateParser("{{ 'Hello' }}{{ @user }}");
        var result = parser.Parse();

        result.Should().HaveCount(2);
        result[0].Should().BeOfType<LiteralNode>()
                 .Which.Value.Should().Be("Hello");

        result[1].Should().BeOfType<VariableNode>()
                 .Which.Value.Should().Be("user");
    }

    [Fact]
    public void Parse_ShouldParse_FunctionWithParameter()
    {
        var parser = CreateParser("{{ concat 'hello' 'world' }}");
        var result = parser.Parse();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<FunctionNode>()
                 .Which.Name.Should().Be("concat");

        var functionNode = (FunctionNode)result[0];
        functionNode.Parameters.Should().HaveCount(2);
        functionNode.Parameters[0].Should().BeOfType<LiteralNode>()
                                  .Which.Value.Should().Be("hello");
        functionNode.Parameters[1].Should().BeOfType<LiteralNode>()
                                  .Which.Value.Should().Be("world");
    }

    [Fact]
    public void Parse_ShouldParse_FunctionWithSubexpression()
    {
        var parser = CreateParser("{{ uppercase (concat 'hello' 'world') }}");
        var result = parser.Parse();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<FunctionNode>()
                 .Which.Name.Should().Be("uppercase");

        var functionNode = (FunctionNode)result[0];
        functionNode.Parameters.Should().HaveCount(1);
        functionNode.Parameters[0].Should().BeOfType<FunctionNode>()
                                  .Which.Name.Should().Be("concat");
    }

    [Fact]
    public void Parse_ShouldParseValidBlockWithText()
    {
        var parser = CreateParser("{{ #repeat 3 }} Hello {{ /repeat }}");
        var result = parser.Parse();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<BlockNode>()
                 .Which.Name.Should().Be("repeat");

        var blockNode = (BlockNode)result[0];
        blockNode.Parameters.Should().HaveCount(1);
        blockNode.Parameters[0].Should().BeOfType<LiteralNode>()
            .Which.Value.Should().Be("3");
        blockNode.Body.Should().HaveCount(1);
        blockNode.Body[0].Should().BeOfType<TextNode>()
            .Which.Value.Should().Be(" Hello ");
    }

    [Fact]
    public void Parse_ShouldParseValidBlockWithInnerExpression()
    {
        var parser = CreateParser("{{ #repeat 3 }}{{ concat 'hello' 'world' }}{{ /repeat }}");
        var result = parser.Parse();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<BlockNode>()
                 .Which.Name.Should().Be("repeat");

        var blockNode = (BlockNode)result[0];
        blockNode.Body.Should().HaveCount(1);
        blockNode.Body[0].Should().BeOfType<FunctionNode>()
                         .Which.Name.Should().Be("concat");
    }

    [Fact]
    public void Parse_ShouldFail_WhenExpressionContainsMoreThanOneLiteralNode()
    {
        var parser = CreateParser("{{ 'extra' @user }}");

        var act = () => parser.Parse();

        act.Should().Throw<TemplateParsingException>();
    }

    [Fact]
    public void Parse_ShouldFail_WhenExpressionContainsMoreThanOneVariableNode()
    {
        var parser = CreateParser("{{ @user 'extra' }}");

        var act = () => parser.Parse();

        act.Should().Throw<TemplateParsingException>();
    }

    [Fact]
    public void Parse_ShouldFail_WhenBlockAppearsInSubExpression()
    {
        var parser = CreateParser("{{ ( #repeat 3 'text' ) }}");

        var act = () => parser.Parse();

        act.Should().Throw<TemplateParsingException>();
    }

    private static Parser CreateParser(string template)
    {
        var lexer = new Lexer(template);
        var tokens = lexer.Tokenize();
        return new Parser(tokens.ToList());
    }
}
