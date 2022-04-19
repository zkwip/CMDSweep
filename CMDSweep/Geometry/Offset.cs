namespace CMDSweep.Geometry;

record struct Offset
{
    public int X;
    public int Y;
    public Offset(int x, int y)
    {
        X = x; 
        Y = y; 
    }

    public static Offset ToPoint(Point newOrigin) => new(newOrigin.X, newOrigin.Y);
    public static Offset FromPoint(Point newOrigin) => new(-newOrigin.X, -newOrigin.Y);
    public static Offset FromChange(Point oldp, Point newp) => new(newp.X - oldp.X, newp.Y - oldp.Y);
}
