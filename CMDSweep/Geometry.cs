﻿using System;
using System.Collections.Generic;

namespace CMDSweep;
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
class Rectangle
{
    public int Left;
    public int Top;
    public int Width;
    public int Height;

    public override string ToString() => String.Format("({0} to {1}, w: {2}, h: {3})", TopLeft, BottomRight, Width, Height);

    public int Right => Left + Width;
    public int Bottom => Top + Height;

    public int CenterLine => Left + Width / 2;
    public int MidLine => Top + Height / 2;

    public Point TopLeft => new(Left, Top);
    public Point BottomRight => new(Right, Bottom);
    public Point TopRight => new(Right, Top);
    public Point BottomLeft => new(Left, Bottom);
    public Point Center => new(Left + Width / 2, Top + Height / 2);

    public static Rectangle Zero => new(0, 0, 0, 0);

    public LinearRange HorizontalRange => new(Left, Width);
    public LinearRange VerticalRange => new(Top, Height);

    public int Area => Width * Height;

    public Rectangle(int x, int y, int width, int height)
    {
        Left = x;
        Top = y;
        Width = width;
        Height = height;

        Fix();
    }
    public Rectangle(LinearRange hor, LinearRange ver)
    {
        Left = hor.Start;
        Top = ver.Start;
        Width = hor.Length;
        Height = ver.Length;

        Fix();
    }

    public Rectangle(Point topleft, Point bottomright)
    {
        Left = topleft.X;
        Top = topleft.Y;
        Width = bottomright.X - topleft.X;
        Height = bottomright.Y - topleft.Y;

        Fix();
    }

    public bool Fix()
    {
        if (Width >= 0 || Height >= 0) return false;
        if (Width < 0)
        {
            Left += Width;
            Width = -Width;
        }
        if (Height < 0)
        {
            Top += Height;
            Height = -Height;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rectangle viewport &&
            Left == viewport.Left &&
            Top == viewport.Top &&
            Width == viewport.Width &&
            Height == viewport.Height;
    }

    public override int GetHashCode() => HashCode.Combine(Left, Top, Width, Height);

    public Rectangle Clone() => new(Left, Top, Width, Height);
    public Rectangle Grow(int left, int top, int right, int bottom) => new(
            Left - left, 
            Top - top, 
            Width + left + right, 
            Height + top + bottom
        );
    public Rectangle Grow(int size) => Grow(size, size, size, size);

    public Rectangle Shrink(int left, int top, int right, int bottom) => Grow(-left, -top, -right, -bottom);
    public Rectangle Shrink(int size) => Grow(-size, -size, -size, -size);
    public bool Contains(Point p) => Contains(p.X, p.Y);
    public bool Contains(int x, int y) => x >= Left && y >= Top && x < Right && y < Bottom;

    public bool Contains(Rectangle r) => Contains(r.TopLeft) && Contains(r.BottomRight.Shifted(-1,-1));
    public Rectangle Intersect(Rectangle other)
    {
        int l = Left > other.Left ? Left : other.Left;
        int t = Top > other.Top ? Top : other.Top;
        int r = Right < other.Right ? Right : other.Right;
        int b = Bottom < other.Bottom ? Bottom : other.Bottom;

        if (b < t || r < l) return Rectangle.Zero;
        return new Rectangle(l, t, r - l, b - t);
    }

    public void ForAll(Action<int, int> callback)
    {
        for (int x = Left; x < Right; x++)
        {
            for (int y = Top; y < Bottom; y++) callback(x, y);
        }
    }

    public void ForAll(Action<Point> callback)
    {
        for (int x = Left; x < Right; x++)
        {
            for (int y = Top; y < Bottom; y++) callback(new(x, y));
        }
    }

    public Offset CenterOn(Point p) => Shift(Offset.FromChange(Center, p));
    public Offset ShiftTo(Point p) => Shift(Offset.FromChange(TopLeft, p));

    public Offset Shift(Offset o) => Shift(o.X, o.Y);
    public Offset Shift(int x, int y)
    {
        Left += x;
        Top += y;
        return new Offset(x, y);

    }

    public Rectangle Shifted(Offset o) => Shifted(o.X, o.Y);
    public Rectangle Shifted(int x, int y)
    {
        Rectangle r = this.Clone();
        r.Shift(x, y);
        return r;
    }

    public Offset OffsetOutOfBounds(Point p)
    {
        int x = 0;
        int y = 0;

        if (p.X < Left) x = p.X - Left;
        if (p.Y < Top) y = p.Y - Top;
        if (p.X >= Right) x = p.X - Right + 1;
        if (p.Y >= Bottom) y = p.Y - Bottom + 1;

        return new Offset(x, y);
    }
}

class Offset : Point {
    public Offset(int x, int y) : base(x, y) { }

    public static Offset ToPoint(Point newOrigin) => new(newOrigin.X, newOrigin.Y);
    public static Offset FromPoint(Point newOrigin) => new(-newOrigin.X, -newOrigin.Y);
    public static Offset FromChange(Point oldp, Point newp) => new(newp.X - oldp.X, newp.Y - oldp.Y);

    public new Offset Clone() => new(X, Y);
}

class Functions
{
    internal static TOut Apply<TIn, TOut>(TOut zero, IEnumerable<TIn> set, Func<TOut, TOut, TOut> func, Func<TIn, TOut> map)
    {
        foreach (TIn item in set) zero = func(zero, map(item));
        return zero;
    }
}