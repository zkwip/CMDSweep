using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.IO;
using CMDSweep.Rendering;
using CMDSweep.Layout.Text;

namespace CMDSweep.Views.Help;

class HelpController : IViewController
{ 
    private TextRenderBox _helpTextBox;
    private readonly IChangeableTypeVisualizer<TextRenderBox,Rectangle> _visualizer;
    private readonly IRenderer _renderer;

    public MineApp App { get; }

    internal HelpController(MineApp app)
    {
        App = app;
        _renderer = App.Renderer;
        StyleData styleData = App.Settings.GetStyle("menu");

        _visualizer = new TextRenderBoxVisualizer(_renderer, App.Settings, styleData);
        _helpTextBox = new TextRenderBox(Storage.LoadHelpFile(), Rectangle.Zero)
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
        InputAction ia = ConsoleInputReader.ReadAction();
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
        _visualizer.Visualize(_helpTextBox, _renderer.Bounds.Shrink(4, 2, 4, 2));
    }
}