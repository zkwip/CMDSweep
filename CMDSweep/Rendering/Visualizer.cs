using CMDSweep.Data;

namespace CMDSweep.Rendering;
abstract class Visualizer<TState> : IVisualizer
{
    private RefreshMode ModeWaiting = RefreshMode.None;
    private bool IsRendering = false;

    internal StyleData HideStyle;
    IViewController IVisualizer.Controller => Controller;
    public IViewController Controller { get; }
    public TState? LastState { get; private set; }
    public TState? CurrentState { get; private set; }

    public IRenderer Renderer => Controller.App.Renderer;
    public GameSettings Settings => Controller.App.Settings;
    public SaveData SaveData => Controller.App.SaveData;
    public GameApp App => Controller.App;

    abstract public void RenderFull();
    abstract public void RenderChanges();
    abstract public bool CheckResize();
    abstract public void Resize();
    abstract public bool CheckScroll();
    abstract public void Scroll();
    abstract public bool CheckFullRefresh();
    abstract public TState RetrieveState();

    public Visualizer(IViewController ctrl)
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
