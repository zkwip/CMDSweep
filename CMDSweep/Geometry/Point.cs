using System;

namespace CMDSweep.Geometry;

record struct Point
{
    public readonly int X;
    public readonly int Y;

    public static Point Origin => new(0, 0);

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => String.Format("({0}, {1})", X, Y);

    public Point Shift(int dx, int dy) => new(X + dx, Y + dy);

    public Point Shift(Offset offset) => new(X + offset.X, Y + offset.Y);

    public Point Scale(Scale dimensions) => new(X * dimensions.Width, Y * dimensions.Height);

    public Point ScaleBack(Scale dimensions) => new(X / dimensions.Width, Y / dimensions.Height);
}
