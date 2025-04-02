using System;

namespace lab2.Models;

public static class DoubleUtil
{
    private const double Epsilon = 0.000001;
    public static bool AreClose(double value1, double value2)
    {
        return Math.Abs(value1 - value2) < Epsilon;
    }
}