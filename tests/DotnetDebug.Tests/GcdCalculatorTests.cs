using System;
using DotnetDebug;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class GcdCalculatorTests
{
    [Test]
    [Arguments(48L, 18L, 6L)]
    [Arguments(-48L, 18L, 6L)]
    [Arguments(0L, 12L, 12L)]
    public async Task ComputeGcd_TwoNumbers(long a, long b, long expected)
    {
        var result = GcdCalculator.ComputeGcd(a, b);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ComputeGcd_MultipleNumbers()
    {
        var result = GcdCalculator.ComputeGcd(new long[] { 48, 18, 30 });
        await Assert.That(result).IsEqualTo(6L);
    }

    [Test]
    public async Task ComputeGcd_AllZeros_Throws()
    {
        await Assert.That(() => GcdCalculator.ComputeGcd(new long[] { 0, 0, 0 }))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task ComputeGcd_Empty_Throws()
    {
        await Assert.That(() => GcdCalculator.ComputeGcd(Array.Empty<long>()))
            .Throws<ArgumentException>();
    }
}
