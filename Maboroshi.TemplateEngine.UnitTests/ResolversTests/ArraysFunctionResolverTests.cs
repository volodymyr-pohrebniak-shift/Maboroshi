using FluentAssertions;
using Maboroshi.TemplateEngine.FunctionResolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maboroshi.TemplateEngine.UnitTests.ResolversTests;

public class ArraysFunctionResolverTests
{
    private readonly ArraysFunctionResolver _resolver = new();

    [Fact]
    public void Array_ShouldCreateArray_WithGivenValues()
    {
        var result = _resolver.TryResolve("array", new StringReturn("a"), new StringReturn("b"), new StringReturn("c"));

        result.Should().BeOfType<ArrayReturn<ReturnType>>()
              .Which.Values.Should().HaveCount(3)
              .And.Contain(new StringReturn("a"))
              .And.Contain(new StringReturn("b"))
              .And.Contain(new StringReturn("c"));
    }

    [Fact]
    public void OneOf_ShouldReturnRandomElement_FromArray()
    {
        var array = new ArrayReturn<ReturnType>([new StringReturn("x"), new StringReturn("y"), new StringReturn("z")]);
        var result = _resolver.TryResolve("oneof", array);

        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Match(value => value == "x" || value == "y" || value == "z");
    }

    [Fact]
    public void SomeOf_ShouldReturnRandomSubset()
    {
        var array = new ArrayReturn<ReturnType>([new StringReturn("a"), new StringReturn("b"), new StringReturn("c"), new StringReturn("d")]);
        var result = _resolver.TryResolve("someof", array, new StringReturn("1"), new StringReturn("3"));

        result.Should().BeOfType<ArrayReturn<ReturnType>>()
              .Which.Values.Should().HaveCountGreaterThanOrEqualTo(1)
              .And.HaveCountLessThanOrEqualTo(3);
    }

    [Fact]
    public void Join_ShouldConcatenateArray_WithSeparator()
    {
        var array = new ArrayReturn<ReturnType>([new StringReturn("a"), new StringReturn("b"), new StringReturn("c")]);
        var result = _resolver.TryResolve("join", array, new StringReturn(","));

        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("a,b,c");
    }

    [Fact]
    public void Join_ShouldUseWhitespaceSeparator_WhenNoSeparatorIsProvided()
    {
        var array = new ArrayReturn<ReturnType>([new StringReturn("a"), new StringReturn("b"), new StringReturn("c")]);
        var result = _resolver.TryResolve("join", array);

        result.Should().BeOfType<StringReturn>()
              .Which.Value.Should().Be("a b c");
    }

    [Fact]
    public void Slice_ShouldReturnCorrectSubset_OfArray()
    {
        var array = new ArrayReturn<ReturnType>([new StringReturn("a"), new StringReturn("b"), new StringReturn("c"), new StringReturn("d")]);
        var result = _resolver.TryResolve("slice", array, new StringReturn("1"), new StringReturn("3"));

        result.Should().BeOfType<ArrayReturn<ReturnType>>()
              .Which.Values.Should().BeEquivalentTo([new StringReturn("b"), new StringReturn("c")]);
    }

    [Fact]
    public void Sort_ShouldSortArray_AscendingByDefault()
    {
        var array = new ArrayReturn<ReturnType>([new StringReturn("c"), new StringReturn("a"), new StringReturn("b")]);
        var result = _resolver.TryResolve("sort", array);

        result.Should().BeOfType<ArrayReturn<ReturnType>>()
              .Which.Values.Should().BeEquivalentTo([new StringReturn("a"), new StringReturn("b"), new StringReturn("c")], options => options.WithStrictOrdering());
    }

    [Fact]
    public void Sort_ShouldSortArray_InDescendingOrder()
    {
        var array = new ArrayReturn<ReturnType>([new StringReturn("c"), new StringReturn("a"), new StringReturn("b")]);
        var result = _resolver.TryResolve("sort", array, new StringReturn("desc"));

        result.Should().BeOfType<ArrayReturn<ReturnType>>()
              .Which.Values.Should().BeEquivalentTo([new StringReturn("c"), new StringReturn("b"), new StringReturn("a")], options => options.WithStrictOrdering());
    }

    [Fact]
    public void Reverse_ShouldReturnArray_InReversedOrder()
    {
        var array = new ArrayReturn<ReturnType>([new StringReturn("a"), new StringReturn("b"), new StringReturn("c")]);
        var result = _resolver.TryResolve("reverse", array);

        result.Should().BeOfType<ArrayReturn<ReturnType>>()
              .Which.Values.Should().BeEquivalentTo([new StringReturn("c"), new StringReturn("b"), new StringReturn("a")], options => options.WithStrictOrdering());
    }
}