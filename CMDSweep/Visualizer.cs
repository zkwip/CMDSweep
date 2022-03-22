namespace CMDSweep;
abstract class Visualizer<TState> : IVisualizer
{
    private RefreshMode ModeWaiting = RefreshMode.None;
    private bool IsRendering = false;

    internal StyleData HideStyle;
    internal Controller Controller;
    internal TState? LastState { get; private set; }
    internal TState? CurrentState { get; private set; }

    internal IRenderer Renderer => Controller.App.Renderer;
    internal GameSettings Settings => Controller.App.Settings;
    internal SaveData SaveData => Controller.App.SaveData;
    internal GameApp App => Controller.App;

    abstract internal void RenderFull();
    abstract internal void RenderChanges();
    abstract internal bool CheckResize();
    abstract internal void Resize();
    abstract internal bool CheckScroll();
    abstract internal void Scroll();
    abstract internal bool CheckFullRefresh();
    abstract internal TState RetrieveState();

    public Visualizer(Controller ctrl)
    {
        Controller = ctrl;
        HideStyle = Settings.GetStyle("menu");
    }

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

                CurrentState = RetrieveState();

                // Decide what to render
                if (CurrentState == null)
                    continue; // Skip; Nothing to render

                if (CheckResize())
                    mode = RefreshMode.Full; // Console changed size

                if (LastState == null)
                    mode = RefreshMode.Full; // No history: Full render

                else if (mode == RefreshMode.ChangesOnly) // else to implicitly exclude the case where prevGS is null
                {
                    if (CheckScroll()) mode = RefreshMode.Scroll;
                    if (CheckFullRefresh()) mode = RefreshMode.Full;
                }

                //Render
                if (mode == RefreshMode.Full)
                {
                    Resize();
                    RenderFull();
                }
                else
                {
                    if (mode == RefreshMode.Scroll) Scroll();
                    RenderChanges();
                }
                LastState = CurrentState;
            }
        }

        IsRendering = false;
        return true;
    }
}

interface IVisualizer
{
    internal bool Visualize(RefreshMode mode);
}