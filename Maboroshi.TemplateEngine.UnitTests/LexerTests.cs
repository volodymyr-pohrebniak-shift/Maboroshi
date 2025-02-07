namespace Maboroshi.TemplateEngine.UnitTests;

public class LexerTests
{
    [Fact]
    public void Lexer_Should_Parse_String()
    {
        var str = @"{
  ""userId"": ""text"",
  ""name"": ""{{queryParam 'name' 'John'}}"",
  ""var"": {{@test}},
  ""oneItem"": ""{{oneOf (array 'item1' 'item2' 'item3')}}"", 
}";

        var lexer = new Lexer(str);

        var tokens = lexer.Tokenize();

        Assert.NotEmpty(tokens);
    }

    [Fact]
    public void Lexer_Should_Parse_Text()
    {
        var str = "test";

        var lexer = new Lexer(str);

        var tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count());
        Assert.Equal(TokenType.TEXT, tokens.First().TokenType);
    }
}