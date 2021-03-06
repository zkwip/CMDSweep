using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;

namespace CMDSweep.Layout.Popup;

internal class PopupVisualizer<TPlaceable> : IChangeableTypeVisualizer<TPlaceable> where TPlaceable : IPlaceable
{
    private readonly GameSettings _settings;
    private readonly IRenderer _renderer;
    private readonly StyleData _styleData;
    private readonly ITypeVisualizer<TPlaceable, Rectangle> _contentVisualizer;

    private Rectangle? _lastShape;

    public PopupVisualizer(IRenderer renderer, GameSettings settings, ITypeVisualizer<TPlaceable, Rectangle> contentVisualizer)
    {
        _settings = settings;
        _renderer = renderer;
        _contentVisualizer = contentVisualizer;
        _styleData = settings.GetStyle("popup");
    }

    public void Visualize(TPlaceable item)
    {
        Rectangle shape = Rectangle.Centered(_renderer.Bounds.Center, item.ContentDimensions);
        RenderPopupAroundShape(shape);
        _contentVisualizer.Visualize(item, shape);
        _lastShape = shape;

    }

    public void VisualizeChanges(TPlaceable item, TPlaceable oldItem)
    {
        Rectangle shape = Rectangle.Centered(_renderer.Bounds.Center, item.ContentDimensions);

        if (shape != _lastShape)
        {
            Visualize(item);
            _lastShape = shape;
            return;
        }

        if (_contentVisualizer is IChangeableTypeVisualizer<TPlaceable, Rectangle> changeable)
        {
            if (shape == _lastShape)
            {
                changeable.VisualizeChanges(item, shape, oldItem, shape);
                return;
            }
        }

        _contentVisualizer.Visualize(item, shape);
    }

    private void RenderPopupAroundShape(Rectangle rect)
    {
        int xpad = _settings.Dimensions["popup-padding-x"];
        int ypad = _settings.Dimensions["popup-padding-y"];

        rect.CenterOn(_renderer.Bounds.Center);
        RenderPopupBox(_styleData, rect.Grow(xpad, ypad, xpad, ypad), "popup-border");
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
