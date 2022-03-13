namespace CMDSweep;

abstract class Controller
{
    internal GameApp App;
    internal Controller(GameApp app)
    {
        App = app;
    }
    internal void Visualize(RefreshMode mode) => Visualizer.Visualize(mode);

    internal IVisualizer Visualizer;
    abstract internal bool Step();
    internal bool Active => App.CurrentController == this;
}