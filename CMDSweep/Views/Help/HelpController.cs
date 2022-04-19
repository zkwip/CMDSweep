using CMDSweep.Geometry;
using CMDSweep.IO;
using CMDSweep.Layout;

namespace CMDSweep.Views.Help;

class HelpController : Controller
{
    internal TextRenderBox Box;

    internal HelpController(GameApp app) : base(app)
    {
        Visualizer = new HelpVisualizer(this);
        Box = new TextRenderBox(Storage.LoadHelpFile(), Rectangle.Zero)
        {
            Wrap = true,
            VerticalOverflow = false,
            HorizontalOverflow = false,
            HorizontalAlign = HorzontalAlignment.Left,
            VerticalAlign = VerticalAlignment.Top,
        };
    }

    internal override bool Step()
    {
        InputAction ia = App.ReadAction();
        switch (ia)
        {
            case InputAction.Quit: App.ShowMainMenu(); return true;
            case InputAction.Up: TryScrollUp(); return true;
            case InputAction.Down: TryScrollDown(); return true;
            case InputAction.NewGame: App.BControl.NewGame(); return true;
            default: break;
        }
        App.Refresh(RefreshMode.ChangesOnly);
        return true;
    }

    private void TryScrollDown()
    {
        Box = Box.Clone();
        Box.ScrollDown();
    }

    private void TryScrollUp()
    {
        Box = Box.Clone();
        Box.ScrollUp();
    }
}