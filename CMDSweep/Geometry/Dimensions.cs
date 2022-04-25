namespace CMDSweep.Geometry;

public record struct Dimensions
{
    public int Width;
    public int Height;

    public Dimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static Dimensions Zero => new(0, 0);
}
