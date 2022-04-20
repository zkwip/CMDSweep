namespace CMDSweep.Rendering;

interface ITypeVisualizer<T>
{
    public void Visualize(T item, RefreshMode mode = RefreshMode.Full);
}