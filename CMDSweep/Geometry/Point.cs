using System;

namespace CMDSweep.Geometry;

class Point
{
    public int X;
    public int Y;

    public static Point Origin => new(0, 0);

    public Point(int x, int y) { X = x; Y = y; }
    public Point Clone() => new(X, Y);

    public override string ToString() => String.Format("({0}, {1})", X, Y);
    public override bool Equals(object? obj)
    {
        return obj is Point p &&
               X == p.X &&
               Y == p.Y;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public void Shift(int x, int y)
    {
        X += x;
        Y += y;
    }

    public Point Shifted(int x, int y) { Point p = this.Clone(); p.Shift(x, y); return p; }
}
