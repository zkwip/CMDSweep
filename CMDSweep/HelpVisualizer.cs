namespace CMDSweep;

class HelpVisualizer : Visualizer<TextBox>
{
    public HelpVisualizer(HelpController hctrl) : base(hctrl) { }
    internal override bool CheckFullRefresh() => LastState!.Text != CurrentState!.Text;
    internal override bool CheckResize() => !CurrentState!.Bounds.Equals(Renderer.Bounds.Shrink(2));
    internal override bool CheckScroll() => true;
    internal override void RenderChanges()
    {
        CurrentState!.Render(Renderer, HideStyle, true);
    }
    internal override void RenderFull()
    {
        Renderer.ClearScreen(HideStyle);
        CurrentState!.Render(Renderer, HideStyle, true);
    }

    internal override void Resize()
    {
        CurrentState!.Bounds = Renderer.Bounds.Shrink(2);
    }

    internal override TextBox RetrieveState() => ((HelpController)Controller).Box;
    internal override void Scroll() { }
}
class HelpController : Controller
{
    internal TextBox Box;

    internal HelpController(GameApp app) : base(app)
    {
        Visualizer = new HelpVisualizer(this);
        Box = new TextBox(Storage.LoadHelpFile(), Rectangle.Zero)
        {
            HorizontalFlow = Overflow.Wrap,
            VerticalFlow = Overflow.Scroll,
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