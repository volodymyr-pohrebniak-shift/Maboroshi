using FluentAssertions;

namespace Maboroshi.TemplateEngine.UnitTests;

public class TemplateEngineTests
{
    [Fact]
    public void TemplateEngineShouldWork()
    {
        const string str = """
                           {
                             "userId": "text",
                             "otherText": 1,
                             "upper": {{ uppercase 'test' }}"
                           }
                           """;
        var template = TemplateGenerator.CreateTemplate(str);
        var result = template.Compile();

        Assert.NotNull(result );
    }

    [Fact]
    public void TemplateEngineShouldWorkWithIfStatements()
    {
        const string str = """
                           {
                             {{ #if 'a' }}
                                Hello World
                             {{ elsif '' }}
                                Other hello
                             {{ else }}
                                Not Hello
                             {{ /end }}
                           }
                           """;
        var template = TemplateGenerator.CreateTemplate(str);
        var result = template.Compile();

        Assert.NotNull(result);
    }

    [Fact]
    public void TemplateEngineShouldWorkWithRepeatBlock()
    {
        const string str = """
                           {
                             "users": [
                               {{ #repeat 2 ',' }}
                                  {
                                     "id": {{ @index }},
                                     "firstName": "{{ firstName }}"
                                  }
                               {{ /end }}
                             ]
                           }
                           """;
        var template = TemplateGenerator.CreateTemplate(str);
        var result = template.Compile();

        Assert.NotNull(result);
    }

    [Fact]
    public void Compile_ShouldReturnText_WhenItContainsClosingBraces()
    {
        Compile("{\"a\":{\"b\":1}}").Should().Be("{\"a\":{\"b\":1}}");
    }

    [Fact]
    public void Compile_ShouldKeepSpaces_AroundInlineExpressions()
    {
        Compile("Hello {{ uppercase 'x' }}!").Should().Be("Hello X!");
    }

    [Fact]
    public void Compile_ShouldParseSubExpressions_EndingWithNumberOrFunctionName()
    {
        Compile("{{ add (add 2 3) 1 }}").Should().Be("6");
        Compile("{{ substr (now) 0 4 }}").Should().Be(DateTime.UtcNow.Year.ToString());
    }

    [Fact]
    public void Compile_ShouldRemoveLines_WithStandaloneBlockTags()
    {
        const string template = "[\n  {{#repeat 2 ','}}\n  {{@index}}\n  {{/end}}\n]";

        Compile(template).Should().Be("[\n  0," + Environment.NewLine + "  1\n]");
    }

    [Theory]
    [InlineData("{{#if 'y'}}A{{ uppercase 'b' }}C{{else}}X{{/end}}", "ABC")]
    [InlineData("{{#if ''}}A{{else}}X{{ uppercase 'y' }}Z{{/end}}", "XYZ")]
    [InlineData("{{#if ''}}A{{elsif 'y'}}B{{ uppercase 'c' }}{{else}}C{{/end}}", "BC")]
    [InlineData("{{#if ''}}A{{elsif ''}}B{{else}}C{{/end}}", "C")]
    [InlineData("{{#if ''}}A{{elsif ''}}B{{/end}}", "")]
    public void Compile_ShouldRenderWholeMatchingBranch_OfIfBlock(string template, string expected)
    {
        Compile(template).Should().Be(expected);
    }

    [Fact]
    public void Compile_ShouldFail_WhenElseIsNotLastClause()
    {
        var act = () => Compile("{{#if ''}}A{{else}}B{{elsif 'y'}}C{{/end}}");

        act.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData("'false'", "no")]
    [InlineData("'FALSE'", "no")]
    [InlineData("'0'", "no")]
    [InlineData("0", "no")]
    [InlineData("''", "no")]
    [InlineData("(subtract 2 2)", "no")]
    [InlineData("'true'", "yes")]
    [InlineData("'abc'", "yes")]
    [InlineData("(add 1 1)", "yes")]
    [InlineData("(array 'a')", "yes")]
    public void Compile_ShouldEvaluateIfCondition(string condition, string expected)
    {
        Compile("{{#if " + condition + "}}yes{{else}}no{{/end}}").Should().Be(expected);
    }

    [Fact]
    public void Compile_ShouldShareVariables_WithinBlockBody()
    {
        Compile("{{#each (array 'a' 'b')}}{{var 'x' @this}}{{@x}}{{/end}}").Should().Be("ab");
        Compile("{{#if 'y'}}{{var 'x' 'v'}}{{@x}}{{/end}}").Should().Be("v");
    }

    [Theory]
    [InlineData("{{#repeat 2}}{{var 'x' 'v'}}{{/end}}{{@x}}")]
    [InlineData("{{#repeat 2}}a{{/end}}{{@index}}")]
    [InlineData("{{#each (array 'a')}}{{var 'x' 'v'}}{{/end}}{{@x}}")]
    public void Compile_ShouldNotLeakVariables_OutOfBlock(string template)
    {
        var act = () => Compile(template);

        act.Should().Throw<Exception>().WithMessage("Variable * doesn't exist");
    }

    [Theory]
    [InlineData("{{ toFixed 7 0 }}", "7")]
    [InlineData("{{ padStart '42' 5 }}", "00042")]
    [InlineData("{{ mod 10 3 }}", "1")]
    public void Compile_ShouldResolveFunctions_RegardlessOfNameCase(string template, string expected)
    {
        Compile(template).Should().Be(expected);
    }

    [Fact]
    public void Compile_ShouldFormatNow()
    {
        Compile("{{ now 'yyyy' }}").Should().Be(DateTime.UtcNow.Year.ToString());
    }

    [Theory]
    [InlineData("{{ array 1 2 3 }}", "1,2,3")]
    [InlineData("{{ array (array 'a' 'b') 'c' }}", "a,b,c")]
    [InlineData("{{ split 'a b' }}", "a,b")]
    [InlineData("{{#each (array 'x')}}{{ array @this 'y' }}{{/end}}", "x,y")]
    [InlineData("{{ join (array (add 1 1) (array 'a' 'b')) '-' }}", "2-a,b")]
    public void Compile_ShouldRenderArray_AsCommaSeparatedValues(string template, string expected)
    {
        Compile(template).Should().Be(expected);
    }

    [Theory]
    [InlineData("{{ json (array 'a' 'b') }}", "[\"a\",\"b\"]")]
    [InlineData("{{ json (split 'a b') }}", "[\"a\",\"b\"]")]
    [InlineData("{{ json (array (add 1 1) (array 'x')) }}", "[2,[\"x\"]]")]
    [InlineData("{{ json 'say \"hi\"' }}", "\"say \\\"hi\\\"\"")]
    [InlineData("{{ json 'Київ' }}", "\"Київ\"")]
    [InlineData("{{ json (add 1 2) }}", "3")]
    public void Compile_ShouldRenderValue_AsJson(string template, string expected)
    {
        Compile(template).Should().Be(expected);
    }

    private static string Compile(string template) => TemplateGenerator.CreateTemplate(template).Compile();
}
