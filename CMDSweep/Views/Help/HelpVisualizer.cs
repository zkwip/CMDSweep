using CMDSweep.Layout;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Help;

class HelpVisualizer : Visualizer<TextRenderBox>
{
    public HelpVisualizer(HelpController hctrl) : base(hctrl) { }
    
    public override bool CheckFullRefresh() => LastState!.Text != CurrentState!.Text;
    
    public override bool CheckResize() => !CurrentState!.Bounds.Equals(Renderer.Bounds.Shrink(2));
    
    public override bool CheckScroll() => true;
    
    public override void RenderChanges()
    {
        CurrentState!.Render(Renderer, HideStyle, true);
    }
    
    public override void RenderFull()
    {
        Renderer.ClearScreen(HideStyle);
        CurrentState!.Render(Renderer, HideStyle, true);
    }

    
    public override void Resize()
    {
        CurrentState!.Bounds = Renderer.Bounds.Shrink(2);
    }

    
    public override TextRenderBox RetrieveState() => ((HelpController)Controller).Box;
    
    public override void Scroll() { }
}
