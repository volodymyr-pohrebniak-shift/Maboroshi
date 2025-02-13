using FluentAssertions;
using Maboroshi.TemplateEngine.FunctionResolvers;

namespace Maboroshi.TemplateEngine.UnitTests.ResolversTests;

public class StringsFunctionResolverTests
{
    private readonly StringsFunctionResolver _resolver = new();

    [Fact]
    public void Lowercase_ShouldConvertToLowerCase()
    {
        var result = _resolver.TryResolve("lowercase", new StringReturn("TEST"));
        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("test");
    }

    [Fact]
    public void Uppercase_ShouldConvertToUpperCase()
    {
        var result = _resolver.TryResolve("uppercase", new StringReturn("test"));
        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("TEST");
    }

    [Fact]
    public void Includes_ShouldReturnTrue_WhenSubstringExists()
    {
        var result = _resolver.TryResolve("includes", new StringReturn("hello world"), new StringReturn("world"));
        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("True");
    }

    [Fact]
    public void Includes_ShouldReturnFalse_WhenSubstringDoesNotExist()
    {
        var result = _resolver.TryResolve("includes", new StringReturn("hello world"), new StringReturn("abc"));
        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("False");
    }

    [Fact]
    public void Substr_ShouldReturnSubstring_WithStartIndex()
    {
        var result = _resolver.TryResolve("substr", new StringReturn("abcdef"), new StringReturn("2"));
        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("cdef");
    }

    [Fact]
    public void Substr_ShouldReturnSubstring_WithStartIndexAndLength()
    {
        var result = _resolver.TryResolve("substr", new StringReturn("abcdef"), new StringReturn("2"), new StringReturn("2"));
        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("cd");
    }

    [Fact]
    public void Split_ShouldReturnArray_WhenSeparatorIsSpecified()
    {
        var result = _resolver.TryResolve("split", new StringReturn("a,b,c"), new StringReturn(","));
        result.Should().BeOfType<ArrayReturn<StringReturn>>()
              .Which.Values.Should().BeEquivalentTo([new StringReturn("a"), new StringReturn("b"), new StringReturn("c")]);
    }

    [Fact]
    public void Split_ShouldReturnArray_WhenNoSeparatorIsSpecified_UsesWhitespace()
    {
        var result = _resolver.TryResolve("split", new StringReturn("a b c"));
        result.Should().BeOfType<ArrayReturn<StringReturn>>()
              .Which.Values.Should().BeEquivalentTo([new StringReturn("a"), new StringReturn("b"), new StringReturn("c")]);
    }

    [Fact]
    public void PadStart_ShouldPadWithZeros_ByDefault()
    {
        var result = _resolver.TryResolve("padStart", new StringReturn("42"), new StringReturn("5"));
        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("00042");
    }

    [Fact]
    public void PadStart_ShouldPadWithCustomCharacter()
    {
        var result = _resolver.TryResolve("padStart", new StringReturn("42"), new StringReturn("5"), new StringReturn("*"));
        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("***42");
    }
}