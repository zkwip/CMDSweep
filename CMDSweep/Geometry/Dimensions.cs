namespace CMDSweep.Geometry;

internal record struct Dimensions
{
    public int Width;
    public int Height;

    public Dimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static Dimensions Zero => new(0, 0);

    internal Dimensions ScaleBack(Scale scale) => new(Width / scale.Width, Height / scale.Height);
    internal Dimensions Scale(Scale scale) => new(Width * scale.Width, Height * scale.Height);
}
