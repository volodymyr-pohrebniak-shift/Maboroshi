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
                             {{ #repeat 5 }}
                                INdex: {{ @index }}
                             {{ /end }}
                           }
                           """;
        var template = TemplateGenerator.CreateTemplate(str);
        var result = template.Compile();

        Assert.NotNull(result);
    }
}
