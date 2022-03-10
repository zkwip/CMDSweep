namespace CMDSweep;
abstract internal class Visualizer<T> : IVisualizer
{
    private RefreshMode ModeWaiting = RefreshMode.None; 
    private bool IsRendering = false;

    internal StyleData hideStyle;
    internal Controller Controller;
    internal T? LastState { get; private set; }
    internal T? CurrentState { get; private set; }

    internal IRenderer Renderer => Controller.App.Renderer;
    internal GameSettings Settings => Controller.App.Settings;
    internal SaveData SaveData => Controller.App.SaveData;

    abstract internal void RenderFull();
    abstract internal void RenderChanges();
    abstract internal bool CheckResize();
    abstract internal bool CheckScroll();
    abstract internal void ApplyScroll();
    abstract internal bool CheckFullRefresh();
    abstract internal T RetrieveState();

    bool IVisualizer.Visualize(RefreshMode mode)
    {
        // Indicate there is stuff to redraw
        if (mode > ModeWaiting) ModeWaiting = mode;

        // Defer renderer
        if (IsRendering) return false;
        IsRendering = true;

        // Actual render loop
        while (ModeWaiting != RefreshMode.None)
        {
            lock (Renderer)
            {
                // Reset queue
                mode = ModeWaiting;
                ModeWaiting = RefreshMode.None;

                T? prevGS = LastState;
                T? curGS = RetrieveState();

                // Decide what to render
                if (curGS == null)
                    continue; // Skip; Nothing to render

                CurrentState = curGS;

                if (CheckResize())
                    mode = RefreshMode.Full; // Console changed size

                if (prevGS == null)
                    mode = RefreshMode.Full; // No history: Full render

                else if (mode == RefreshMode.ChangesOnly) // else to implicitly exclude the case where prevGS is null
                {
                    if (CheckScroll()) mode = RefreshMode.Scroll;
                    if (CheckFullRefresh()) mode = RefreshMode.Full;
                }

                //Render
                if (mode == RefreshMode.Full)
                {
                    RenderFull();
                }
                else
                {
                    if (mode == RefreshMode.Scroll) ApplyScroll();

                    RenderChanges();
                }
                LastState = curGS;
            }
        }

        IsRendering = false;
        return true;
    }
}

internal interface IVisualizer
{
    internal bool Visualize(RefreshMode mode);
}