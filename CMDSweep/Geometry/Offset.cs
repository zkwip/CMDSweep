namespace CMDSweep.Geometry;

record struct Offset
{
    public int X;
    public int Y;

    public static Offset Zero => new(0, 0);

    public Offset(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Offset ToPoint(Point newOrigin) => new(newOrigin.X, newOrigin.Y);

    public static Offset FromPoint(Point newOrigin) => new(-newOrigin.X, -newOrigin.Y);

    public static Offset FromChange(Point oldp, Point newp) => new(newp.X - oldp.X, newp.Y - oldp.Y);

    public Offset Shift(Offset offset) => new Offset(X + offset.X, Y + offset.Y);

    public Offset Reverse => new Offset(-X, -Y);

    public Offset Scale(Scale dimensions) => new(X * dimensions.Width, Y * dimensions.Height);

    public Offset ScaleBack(Scale dimensions) => new(X / dimensions.Width, Y / dimensions.Height);
}
