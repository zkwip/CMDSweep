using CMDSweep.Rendering;
using CMDSweep.Data;
using CMDSweep.Geometry;

namespace CMDSweep.Layout.Popup;

internal class PopupVisualizer : ITypeVisualizer<IPopup>
{
    private GameSettings _settings;
    private IRenderer _renderer;
    public PopupVisualizer(IRenderer renderer, GameSettings settings)
    {
        _settings = settings;
        _renderer = renderer;
    }

    public void Visualize(IPopup item, RefreshMode mode = RefreshMode.Full)
    {
        Rectangle shape = Rectangle.Centered(_renderer.Bounds.Center, item.ContentDimensions);

        if (mode == RefreshMode.Full)
            RenderPopupAroundShape(shape);

        item.RenderContent(shape, _renderer);
    }

    private void RenderPopupAroundShape(Rectangle rect)
    {
        int xpad = _settings.Dimensions["popup-padding-x"];
        int ypad = _settings.Dimensions["popup-padding-y"];
        StyleData style = _settings.GetStyle("popup");
        rect.CenterOn(_renderer.Bounds.Center);
        RenderPopupBox(style, rect.Grow(xpad, ypad, xpad, ypad), "popup-border");
    }

    private void RenderPopupBox(StyleData style, Rectangle r, string border)
    {
        _renderer.ClearScreen(style, r);

        r = r.Shrink(0, 0, 1, 1); // since it is exclusive
        r.HorizontalRange.ForEach((i) => _renderer.PrintAtTile(new(i, r.Top), style, _settings.Texts[border + "-side-top"]));
        r.HorizontalRange.ForEach((i) => _renderer.PrintAtTile(new(i, r.Bottom), style, _settings.Texts[border + "-side-bottom"]));

        r.VerticalRange.ForEach((i) => _renderer.PrintAtTile(new(r.Left, i), style, _settings.Texts[border + "-side-left"]));
        r.VerticalRange.ForEach((i) => _renderer.PrintAtTile(new(r.Right, i), style, _settings.Texts[border + "-side-right"]));

        _renderer.PrintAtTile(r.TopLeft, style, _settings.Texts[border + "-corner-tl"]);
        _renderer.PrintAtTile(r.BottomLeft, style, _settings.Texts[border + "-corner-bl"]);
        _renderer.PrintAtTile(r.TopRight, style, _settings.Texts[border + "-corner-tr"]);
        _renderer.PrintAtTile(r.BottomRight, style, _settings.Texts[border + "-corner-br"]);

    }
}
