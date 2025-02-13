namespace Maboroshi.TemplateEngine.UnitTests;

public class TemplateEngineTests
{
    [Fact]
    public void TemplateEngineShouldWork()
    {
        var str = @"{
  ""userId"": ""text"",
  ""otherText"": 1,
  ""upper"": {{ uppercase 'test' }}""
]
}";
        var template = TemplateEngine.CreateTemplate(str);
        var result = template.Compile();

        Assert.NotNull(result );
    }
}
