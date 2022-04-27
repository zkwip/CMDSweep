using CMDSweep.Rendering;

namespace CMDSweep.Views;

interface IViewController
{
    public MineApp App { get; }

    public void Step();
    void ResizeView();
    void Refresh(RefreshMode mode);
}