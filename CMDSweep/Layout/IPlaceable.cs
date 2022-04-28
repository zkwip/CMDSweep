using CMDSweep.Geometry;

namespace CMDSweep.Layout;

internal interface IPlaceable
{
    public Dimensions ContentDimensions { get; }
}