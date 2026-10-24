using System.Globalization;
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
        var act = () => Compile("{{#if ''}}A{{else}}B{{elsif 'y'}}C{{/end}}", strict: true);

        act.Should().Throw<TemplateRuntimeException>();
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
        var act = () => Compile(template, strict: true);

        act.Should().Throw<TemplateRuntimeException>().WithMessage("Variable * doesn't exist");
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
    [InlineData("{{ json (array 1 'a' 2.5 -1) }}", "[1,\"a\",2.5,-1]")]
    public void Compile_ShouldRenderValue_AsJson(string template, string expected)
    {
        Compile(template).Should().Be(expected);
    }

    [Theory]
    [InlineData("{{ number 5 5 }}", "5")]
    [InlineData("{{ number '5' '5' }}", "5")]
    [InlineData("{{ faker 'even' 4 4 }}", "4")]
    [InlineData("{{ faker 'odd' 3 3 }}", "3")]
    [InlineData("{{ faker 'words' 0 }}", "")]
    public void Compile_ShouldPassNumericArguments_ToFaker(string template, string expected)
    {
        Compile(template, strict: true).Should().Be(expected);
    }

    [Fact]
    public void Compile_ShouldUseFakerArguments()
    {
        int.Parse(Compile("{{ number 7 }}", strict: true)).Should().BeGreaterThanOrEqualTo(7);
        Compile("{{ faker 'date.between' '2020-01-01' '2020-01-02' }}", strict: true).Should().StartWith("2020-01-01");
    }

    [Theory]
    [InlineData("{{ faker 'byte' }}")]
    [InlineData("{{ faker 'char' }}")]
    [InlineData("{{ faker 'Name.FirstName' }}")]
    [InlineData("{{ faker 'date.recent' 2 }}")]
    public void Compile_ShouldResolveFakerValues(string template)
    {
        Compile(template, strict: true).Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("{{ faker 'no.such.path' }}", "*no.such.path*")]
    [InlineData("{{ faker 'words' 'abc' }}", "*should be an integer*")]
    public void Compile_ShouldFail_OnInvalidFakerCall(string template, string message)
    {
        var act = () => Compile(template, strict: true);

        act.Should().Throw<TemplateRuntimeException>().WithMessage(message);
    }

    [Theory]
    [InlineData("{{ join (split 'a b') '-' }}", "a-b")]
    [InlineData("{{#each (split 'a b')}}[{{@this}}]{{/end}}", "[a][b]")]
    [InlineData("{{ oneOf (split 'x') }}", "x")]
    [InlineData("{{ reverse (split 'a b') }}", "b,a")]
    [InlineData("{{ sort (split 'b a') }}", "a,b")]
    [InlineData("{{ sort (array 10 9 1) }}", "1,9,10")]
    [InlineData("{{ sort (array '10' 9 1) 'desc' }}", "10,9,1")]
    [InlineData("{{ sort (array 'b' 'a' 'C') }}", "C,a,b")]
    public void Compile_ShouldAcceptAnyArray_InArrayFunctions(string template, string expected)
    {
        Compile(template, strict: true).Should().Be(expected);
    }

    [Theory]
    [InlineData("{{ padStart 42 5 }}", "00042")]
    [InlineData("{{ substr 12345 1 2 }}", "23")]
    [InlineData("{{ uppercase 1.5 }}", "1.5")]
    public void Compile_ShouldAcceptNumbers_InStringFunctions(string template, string expected)
    {
        Compile(template, strict: true).Should().Be(expected);
    }

    [Theory]
    [InlineData("{{ divide 7 2 }}", "3.5")]
    [InlineData("{{ add '1.5' 1 }}", "2.5")]
    [InlineData("{{ ceil 2.3 }}", "3")]
    [InlineData("{{ json (array 1.5) }}", "[1.5]")]
    public void Compile_ShouldNotDependOnServerCulture(string template, string expected)
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("uk-UA");
        try
        {
            Compile(template, strict: true).Should().Be(expected);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Theory]
    [InlineData("(eq (add 2 3) 5)", "yes")]
    [InlineData("(eq '5' 5)", "yes")]
    [InlineData("(eq 'abc' 'abc')", "yes")]
    [InlineData("(eq 'abc' 'abd')", "no")]
    [InlineData("(noteq 'abc' 'abd')", "yes")]
    [InlineData("(gt 'abc' 5)", "no")]
    [InlineData("(lt 'abc' 5)", "no")]
    [InlineData("(gte '5' 5)", "yes")]
    public void Compile_ShouldCompareNumericStrings_AsNumbers(string condition, string expected)
    {
        Compile("{{#if " + condition + "}}yes{{else}}no{{/end}}", strict: true).Should().Be(expected);
    }

    [Fact]
    public void Compile_ShouldRenderFailedNodeAsEmpty_InNonStrictMode()
    {
        Compile("a{{ unknownFn }}b").Should().Be("ab");
        Compile("{{#each (array 'x' 'y')}}[{{ unknownFn }}{{@this}}]{{/end}}").Should().Be("[x][y]");
        Compile("{{ substr 'abc' 10 }}").Should().BeEmpty();
        Compile("{{var 'a' 'ok'}}{{#if 'y'}}{{ unknownFn }}{{/end}}{{@a}}").Should().Be("ok");
    }

    [Theory]
    [InlineData("{{ unknownFn }}")]
    [InlineData("{{ substr 'abc' 10 }}")]
    [InlineData("{{ @missing }}")]
    public void Compile_ShouldThrowRuntimeException_InStrictMode(string template)
    {
        var act = () => Compile(template, strict: true);

        act.Should().Throw<TemplateRuntimeException>();
    }

    [Fact]
    public void CreateTemplate_ShouldHandleLexerErrors_AccordingToStrictMode()
    {
        Compile("text {{ ").Should().Be("text {{ ");

        var act = () => Compile("text {{ ", strict: true);

        act.Should().Throw<TemplateParsingException>();
    }

    private static string Compile(string template, bool strict = false) =>
        TemplateGenerator.CreateTemplate(template, new(strict)).Compile();
}
