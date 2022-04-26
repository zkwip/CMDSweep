using CMDSweep.Data;
using CMDSweep.Layout;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Help;

class HelpVisualizer : IChangeableTypeVisualizer<TextRenderBox>
{
    IRenderer _renderer;
    StyleData _styleData;

    public HelpVisualizer(IRenderer renderer, GameSettings settings)
    {
        _renderer = renderer;
        _styleData = settings.GetStyle("menu");
    }

    public void VisualizeChanges(TextRenderBox state, TextRenderBox old)
    {
        state.RenderScroll(_renderer, state.VerticalScroll - old.VerticalScroll);
    }
    
    public void Visualize(TextRenderBox state)
    {
        _renderer.ClearScreen(_styleData);
        state.Render(_renderer, true);
    }
}
