using CMDSweep.Geometry;

namespace CMDSweep.Rendering;

internal record struct RenderBufferCopyTask
{
    public readonly Rectangle Source;
    public readonly Offset Offset;
    public readonly bool Empty;

    public override string ToString() => $"RenderBuffer Copy from {Source} with offset {Offset}";

    public RenderBufferCopyTask(Rectangle source, Rectangle dest)
    {
        Source = source;
        if (source.Dimensions != dest.Dimensions)
            throw new DimensionMismatchException();

        Offset = Offset.FromChange(source.TopLeft, dest.TopLeft);
        Empty = Source.Area == 0;
    }

    public RenderBufferCopyTask(Rectangle source, Offset offset)
    {
        Source = source;
        Offset = offset;
        Empty = Source.Area == 0;
    }

    public RenderBufferCopyTask()
    {
        Source = Rectangle.Zero;
        Offset = Offset.Zero;
        Empty = true;
    }

    public Rectangle Destination => Source.Shift(Offset);

    public static RenderBufferCopyTask None => new(Rectangle.Zero, Rectangle.Zero);
}
