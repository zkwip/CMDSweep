using CMDSweep.Views;

namespace CMDSweep.Rendering;

interface IVisualizer
{
    internal bool Visualize(RefreshMode mode);
    internal IViewController Controller { get; }
}
