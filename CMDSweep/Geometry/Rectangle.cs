using System;

namespace CMDSweep.Geometry;

record struct Rectangle
{
    public readonly LinearRange HorizontalRange;
    public readonly LinearRange VerticalRange;

    public Rectangle(int x, int y, int width, int height)
    {
        HorizontalRange = new LinearRange(x, width);
        VerticalRange = new LinearRange(y, height);
    }
    public Rectangle(LinearRange hor, LinearRange ver)
    {
        HorizontalRange = hor;
        VerticalRange = ver;
    }

    public Rectangle(Point point1, Point point2)
    {
        HorizontalRange = new LinearRange(point1.X, point2.X - point1.X);
        VerticalRange = new LinearRange(point1.Y, point2.Y - point1.Y);
    }

    public Rectangle(Point topLeft, Dimensions dimensions)
    {
        HorizontalRange = new LinearRange(topLeft.X, dimensions.Width);
        VerticalRange = new LinearRange(topLeft.Y, dimensions.Height);
    }

    internal static Rectangle Centered(Point center, Dimensions dimensions)
    {
        return new Rectangle(Point.Origin, dimensions).CenterOn(center);
    }

    public override string ToString() => String.Format("({0} to {1}, w: {2}, h: {3})", TopLeft, BottomRight, Width, Height);

    public int Left => HorizontalRange.Start;

    public int Top => VerticalRange.Start;

    public int Width => HorizontalRange.Length;

    public int Height => VerticalRange.Length;

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

    public int Area => Width * Height;

    public Dimensions Dimensions => new(Width, Height);

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

    public bool Contains(Rectangle r) => Contains(r.TopLeft) && Contains(r.BottomRight.Shifted(-1, -1));
    
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
    
    public Rectangle CenterOn(Point p) => Shift(Offset.FromChange(Center, p));
    
    public Rectangle ShiftTo(Point p) => Shift(Offset.FromChange(TopLeft, p));

    public Rectangle Shift(Offset o) => Shift(o.X, o.Y);
    
    public Rectangle Shift(int x, int y)
    {
        return new Rectangle(HorizontalRange.Shift(x), VerticalRange.Shift(y));
    }

    public Offset OffsetOutOfBounds(Point p) => new Offset(HorizontalRange.OffsetOutOfBounds(p.X), VerticalRange.OffsetOutOfBounds(p.Y));
}
