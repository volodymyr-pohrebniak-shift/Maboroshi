using FluentAssertions;
using Maboroshi.TemplateEngine.FunctionResolvers;
namespace Maboroshi.TemplateEngine.UnitTests.ResolversTests;

public class MathFunctionResolverTests
{
    private readonly MathFunctionResolver _resolver = new();

    public static IEnumerable<object[]> MathTestData =>
    [
        ["add", 6.0, new double[] { 1, 2, 3 }],
        ["subtract", -4.0, new double[] { 1, 2, 3 }],
        ["multiply", 6.0, new double[] { 1, 2, 3 }],
        ["divide", 0.16666666666666666, new double[] { 1, 2, 3 }],
        ["modulo", 1.0, new double[] { 10, 3 }],
        ["ceil", 2.0, new double[] { 1.1 }],
        ["floor", 1.0, new double[] { 1.9 }],
        ["round", 2.0, new double[] { 1.6 }]
    ];

    [Theory]
    [MemberData(nameof(MathTestData))]
    public void TestMathOperations(string functionName, double expected, params double[] args)
    {
        var returns = args.Select(n => new NumberReturn(n)).ToArray<ReturnType>();
        var result = _resolver.TryResolve(functionName, returns);
        result.Should().BeOfType<NumberReturn>()
              .Which.Value.Should().BeApproximately(expected, 0.000001);
    }

    [Fact]
    public void TestAddWithStringNumbers()
    {
        var result = _resolver.TryResolve("add",
            new StringReturn("2"), new NumberReturn(3));

        result.Should().BeOfType<NumberReturn>()
              .Which.Value.Should().Be(5);
    }

    [Fact]
    public void TestEqTrue()
    {
        var result = _resolver.TryResolve("eq",
            new NumberReturn(5), new NumberReturn(5));

        result.Should().BeOfType<BoolReturn>()
              .Which.Value.Should().BeTrue();
    }

    [Fact]
    public void TestEqFalseDifferentTypes()
    {
        var result = _resolver.TryResolve("eq",
            new NumberReturn(5), new StringReturn("5"));

        result.Should().BeOfType<BoolReturn>()
              .Which.Value.Should().BeFalse();
    }

    [Fact]
    public void TestToFixed()
    {
        var result = _resolver.TryResolve("toFixed",
            new NumberReturn(5.6789), new NumberReturn(2));

        result.Should().BeOfType<NumberReturn>()
              .Which.Value.Should().Be(5.68);
    }

    [Fact]
    public void TestGt()
    {
        var result = _resolver.TryResolve("gt",
            new NumberReturn(5), new NumberReturn(3));

        result.Should().BeOfType<BoolReturn>()
              .Which.Value.Should().BeTrue();
    }

    [Fact]
    public void TestLtFalse()
    {
        var result = _resolver.TryResolve("lt",
            new NumberReturn(5), new NumberReturn(3));

        result.Should().BeOfType<BoolReturn>()
              .Which.Value.Should().BeFalse();
    }
}
