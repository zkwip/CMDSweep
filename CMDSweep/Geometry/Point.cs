using System;

namespace CMDSweep.Geometry;

record struct Point
{
    public readonly int X;
    public readonly int Y;

    public static Point Origin => new(0, 0);

    public Point(int x, int y) { 
        X = x; 
        Y = y; 
    }

    public override string ToString() => String.Format("({0}, {1})", X, Y);

    public Point Shifted(int dx, int dy) => new Point(X + dx, Y + dy);
    public Point Shifted(Offset offset) => new Point(X + offset.X, Y + offset.Y);
}
