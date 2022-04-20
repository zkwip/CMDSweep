using CMDSweep.Geometry;
using CMDSweep.Rendering;

namespace CMDSweep.Layout;

internal interface IRenderable
{
    public void Render(IRenderer Renderer);
}
