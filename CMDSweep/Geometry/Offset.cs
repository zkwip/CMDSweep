namespace CMDSweep.Geometry;

class Offset : Point
{
    public Offset(int x, int y) : base(x, y) { }

    public static Offset ToPoint(Point newOrigin) => new(newOrigin.X, newOrigin.Y);
    public static Offset FromPoint(Point newOrigin) => new(-newOrigin.X, -newOrigin.Y);
    public static Offset FromChange(Point oldp, Point newp) => new(newp.X - oldp.X, newp.Y - oldp.Y);

    public new Offset Clone() => new(X, Y);
}
