namespace CMDSweep.Rendering;

interface IVisualizer
{
    internal bool Visualize(RefreshMode mode);
    internal Controller Controller { get; }
}
