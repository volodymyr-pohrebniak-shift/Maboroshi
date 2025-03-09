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
}
