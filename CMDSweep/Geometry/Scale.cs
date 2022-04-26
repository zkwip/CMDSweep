namespace CMDSweep.Geometry;

internal record struct Scale
{
    public int Width;
    public int Height;

    public Scale(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static Dimensions Zero => new(0, 0);
}