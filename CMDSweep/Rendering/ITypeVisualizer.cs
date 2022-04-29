namespace CMDSweep.Rendering;

interface ITypeVisualizer<TState>
{
    public void Visualize(TState state);
}

interface ITypeVisualizer<TState, TContext>
{
    public void Visualize(TState state, TContext context);
}

interface ITypeVisualizer<TState, TContext1, TContext2>
{
    public void Visualize(TState state, TContext1 context1, TContext2 context2);
}