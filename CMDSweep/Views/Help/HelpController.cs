using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.IO;
using CMDSweep.Data;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Help;

class HelpController : IViewController
{ 
    private TextRenderBox _textBox;
    private HelpVisualizer _visualizer;
    private IRenderer _renderer;

    public GameApp App { get; }

    internal HelpController(GameApp app)
    {
        App = app;
        _renderer = App.Renderer;
        _visualizer = new HelpVisualizer(_renderer, App.Settings);

        StyleData styleData = App.Settings.GetStyle("menu");

        _textBox = new TextRenderBox(Storage.LoadHelpFile(), Rectangle.Zero, styleData)
        {
            Wrap = true,
            VerticalOverflow = false,
            HorizontalOverflow = false,
            HorizontalAlign = HorzontalAlignment.Left,
            VerticalAlign = VerticalAlignment.Top,
        };
    }

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
        _textBox = _textBox.Clone();
        _textBox.ScrollDown();
    }

    private void TryScrollUp()
    {
        _textBox = _textBox.Clone();
        _textBox.ScrollUp();
    }

    public void ResizeView()
    {
        
    }

    public void Refresh(RefreshMode mode)
    {
        _visualizer.Visualize(_textBox);
    }
}