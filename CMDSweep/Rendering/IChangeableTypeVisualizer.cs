namespace CMDSweep.Rendering;

interface IChangeableTypeVisualizer<T> : ITypeVisualizer<T>
{
    public void VisualizeChanges(T newItem, T oldItem);
}