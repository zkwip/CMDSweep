namespace CMDSweep.Rendering;

interface IChangeableTypeVisualizer<T> : ITypeVisualizer<T> where T : IRenderState
{
    public void VisualizeChanges(T newItem, T oldItem);
}

interface IChangeableTypeVisualizer<T1, T2> : ITypeVisualizer<T1, T2> where T1 : IRenderState where T2 : IRenderState
{
    public void VisualizeChanges(T1 newItem1, T2 newItem2, T1 oldItem1, T2 oldItem2);
}

interface IChangeableTypeVisualizer<T1, T2, T3> : ITypeVisualizer<T1, T2, T3> where T1 : IRenderState where T2 : IRenderState where T3 : IRenderState
{
    public void VisualizeChanges(T1 newItem1, T2 newItem2, T3 newItem3, T1 oldItem1, T2 oldItem2, T3 oldItem3);
}