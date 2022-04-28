namespace CMDSweep.Rendering;

interface IChangeableTypeVisualizer<TState> : ITypeVisualizer<TState>
{
    public void VisualizeChanges(TState newItem, TState oldItem);
}

interface IChangeableTypeVisualizer<TState, TContext> : ITypeVisualizer<TState, TContext>
{
    public void VisualizeChanges(TState state, TContext context, TState oldState, TContext oldContext);
}

interface IChangeableTypeVisualizer<TState, TContext1, TContext2> : ITypeVisualizer<TState, TContext1, TContext2>
{
    public void VisualizeChanges(TState state, TContext1 context1, TContext2 context2, TState oldState, TContext1 oldContext1, TContext2 oldContext2);
}