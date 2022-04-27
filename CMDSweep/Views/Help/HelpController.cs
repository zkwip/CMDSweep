using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.IO;
using CMDSweep.Data;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Help;

class HelpController : IViewController
{ 
    private TextRenderBox _helpTextBox;
    private readonly HelpVisualizer _visualizer;
    private readonly IRenderer _renderer;

    public MineApp App { get; }

    internal HelpController(MineApp app)
    {
        App = app;
        _renderer = App.Renderer;
        _visualizer = new HelpVisualizer(_renderer, App.Settings);

        StyleData styleData = App.Settings.GetStyle("menu");

        _helpTextBox = new TextRenderBox(Storage.LoadHelpFile(), Rectangle.Zero, styleData)
        {
            Wrap = true,
            VerticalOverflow = false,
            HorizontalOverflow = false,
            HorizontalAlign = HorzontalAlignment.Left,
            VerticalAlign = VerticalAlignment.Top,
        };

        ResizeView();
    }

    public void Step()
    {
        InputAction ia = App.ReadAction();
        switch (ia)
        {
            case InputAction.Quit: 
                App.ShowMainMenu(); 
                break;

            case InputAction.Up: 
                TryScrollUp(); 
                break;

            case InputAction.Down: 
                TryScrollDown(); 
                break;

            case InputAction.NewGame: 
                App.GameController.NewGame(); 
                break;

            default: break;
        }
    }

    private void TryScrollDown()
    {
        _helpTextBox = _helpTextBox.Clone();
        _helpTextBox.ScrollDown();
    }

    private void TryScrollUp()
    {
        _helpTextBox = _helpTextBox.Clone();
        _helpTextBox.ScrollUp();
    }

    public void ResizeView()
    {
        _helpTextBox.Bounds = _renderer.Bounds.Shrink(4, 2, 4, 2);
    }

    public void Refresh(RefreshMode mode)
    {
        _visualizer.Visualize(_helpTextBox);
    }
}