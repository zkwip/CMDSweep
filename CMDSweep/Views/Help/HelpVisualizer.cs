using CMDSweep.Layout;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Help;

class HelpVisualizer : Visualizer<TextRenderBox>
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

    internal override TextRenderBox RetrieveState() => ((HelpController)Controller).Box;
    internal override void Scroll() { }
}
