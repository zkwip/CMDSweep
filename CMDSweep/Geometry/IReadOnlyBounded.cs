namespace CMDSweep.Geometry;

internal interface IReadOnlyBounded
{
    Rectangle Bounds { get; }
}
internal interface IBounded : IReadOnlyBounded
{
    new Rectangle Bounds { get; set; }

    public void CenterOn(Point p) => Bounds = Bounds.CenterOn(p);

    public void Shift(Offset o) => Bounds = Bounds.Shift(o);
}
