namespace Maboroshi.TemplateEngine.UnitTests;

public class TemplateEngineTests
{
    [Fact]
    public void TemplateEngineShouldWork()
    {
        var str = @"{
  ""userId"": ""text"",
  ""name"": ""{{var 'name' 'John'}}"",
  ""var"": {{@name}},
  ""oneItem"": ""{{oneOf (array 'item1' 'item2' 'item3')}}"",
  ""otherText"": 1,
  ""upper"": {{ uppercase @name }}""
  ""repeat"": [
    {{ #repeat 3 }}
        {{ lowercase 'TEST' }}
        test
{{ /end }}
]
}";

        var template = new Template(str);
        var result = template.Compile();

        Assert.NotNull(result );
    }
}
