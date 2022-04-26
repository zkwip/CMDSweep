using System;

namespace CMDSweep.Geometry;

record struct LinearRange
{
    public readonly int Start;
    public readonly int Length;
    public int End => Start + Length;

    public int OffsetOutOfBounds(int target)
    {
        int offset = 0;

        if (target < Start) offset = target - Start;
        if (target >= End) offset = target - End + 1;

        return offset;
    }

    public LinearRange(int start, int length)
    {
        if (length < 0)
        {
            length = -length;
            start -= length;
        }

        Start = start;
        Length = length;
    }

    public static LinearRange ToEnd(int start, int end) => new(start, end - start);

    public override string ToString() => string.Format("({0} to {1} w: {2})", Start, End, Length);

    public static LinearRange Zero => new(0, 0);

    internal void ForEach(Action<int> func)
    {
        for (int i = Start; i < End; i++) func(i);
    }

    internal LinearRange Shift(int offset) => new(Start + offset, Length);

    internal LinearRange Intersect(LinearRange range) => ToEnd(Math.Max(range.Start, this.Start), Math.Min(range.End, this.End));
}
