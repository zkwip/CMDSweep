namespace CMDSweep;

internal abstract class Controller
{
    internal GameApp App;
    internal Controller(GameApp app) => App = app;
    abstract internal bool Step();
    abstract internal void Visualize(RefreshMode mode);
    internal bool Active => App.CurrentController == this;
}