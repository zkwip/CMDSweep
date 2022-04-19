using System;
using System.Collections.Generic;

namespace CMDSweep.Geometry;

class GeometryFunctions
{
    internal static TOut Apply<TIn, TOut>(TOut zero, IEnumerable<TIn> set, Func<TOut, TOut, TOut> func, Func<TIn, TOut> map)
    {
        foreach (TIn item in set) zero = func(zero, map(item));
        return zero;
    }
}