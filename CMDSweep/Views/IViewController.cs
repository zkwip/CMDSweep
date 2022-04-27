using CMDSweep.Rendering;

namespace CMDSweep.Views;

interface IViewController
{
    public MineApp App { get; }

    public bool Step();
    void ResizeView();
    void Refresh(RefreshMode mode);
}