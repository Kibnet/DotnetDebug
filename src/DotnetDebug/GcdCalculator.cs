using System;
using System.Collections.Generic;

namespace DotnetDebug;

public static class GcdCalculator
{
    public static long ComputeGcd(IEnumerable<long> numbers)
    {
        if (numbers is null)
        {
            throw new ArgumentNullException(nameof(numbers));
        }

        long gcd = 0;
        var hasAny = false;
        var hasNonZero = false;

        foreach (var n in numbers)
        {
            hasAny = true;
            if (n != 0)
            {
                hasNonZero = true;
            }

            gcd = ComputeGcd(gcd, n);
        }

        if (!hasAny)
        {
            throw new ArgumentException("At least one number is required.", nameof(numbers));
        }

        if (!hasNonZero)
        {
            throw new ArgumentException("GCD is undefined for all zeros.", nameof(numbers));
        }

        return gcd;
    }

    public static long ComputeGcd(long a, long b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);

        while (b != 0)
        {
            var t = a % b;
            a = b;
            b = t;
        }

        return a;
    }
}
