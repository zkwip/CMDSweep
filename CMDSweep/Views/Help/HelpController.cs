using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.IO;
using CMDSweep.Data;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Help;

class HelpController : IViewController
{
    internal TextRenderBox Box;

    public GameApp App { get; }

    public HelpVisualizer Visualizer { get; }

    internal HelpController(GameApp app)
    {
        Visualizer = new HelpVisualizer(this);
        App = app;
        StyleData styleData = Settings.GetStyle("menu");

        Box = new TextRenderBox(Storage.LoadHelpFile(), Rectangle.Zero, styleData)
        {
            Wrap = true,
            VerticalOverflow = false,
            HorizontalOverflow = false,
            HorizontalAlign = HorzontalAlignment.Left,
            VerticalAlign = VerticalAlignment.Top,
        };
    }

    public GameSettings Settings => App.Settings;
    public SaveData SaveData => App.SaveData;

    public bool Step()
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