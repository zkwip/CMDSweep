using System;

namespace CMDSweep;

class LinearRange
{
    public int Start;
    public int Length;
    public int End => Start + Length;

    public int OffsetOutOfBounds(int target)
    {
        int offset = 0;

        if (target < Start) offset = target - Start;
        if (target >= End) offset = target - End + 1;

        return offset;
    }

    public LinearRange(int start, int length) { Start = start; Length = length; }
    public static LinearRange ToEnd(int start, int end) => new(start, end - start);
    public LinearRange Clone() => new(Start, Length);

    public override bool Equals(object? obj)
    {
        return obj is LinearRange range &&
               Start == range.Start &&
               Length == range.Length;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Length);
    }
    public override string ToString() => String.Format("({0} to {1} w: {2})", Start, End, Length);
    public static LinearRange Zero => new(0, 0);

    internal void ForEach(Action<int> func)
    {
        for (int i = Start; i < End; i++) func(i);
    }
}
