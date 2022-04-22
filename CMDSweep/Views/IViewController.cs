namespace CMDSweep.Views;

interface IViewController
{
    public GameApp App { get; }

    public bool Step();
    void ResizeView();
    void Refresh(RefreshMode mode);
}