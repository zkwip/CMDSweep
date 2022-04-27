namespace CMDSweep.Rendering;

interface ITypeVisualizer<T>
{
    public void Visualize(T item);
}

interface ITypeVisualizer<T1,T2>
{
    public void Visualize(T1 item1, T2 item2);
}

interface ITypeVisualizer<T1, T2, T3>
{
    public void Visualize(T1 item1, T2 item2, T3 item3);
}