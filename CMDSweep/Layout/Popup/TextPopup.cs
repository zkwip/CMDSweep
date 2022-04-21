using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;

namespace CMDSweep.Layout.Popup;

internal class TextPopup : IPopup
{
    private TextRenderBox _textRenderBox;

    public TextPopup(GameSettings settings, string text, Dimensions d)
    {
        _textRenderBox = new TextRenderBox();
        _textRenderBox.Bounds = new Rectangle(0, 0, d.Width, d.Height);
        _textRenderBox.Text = text;
        _textRenderBox.Bounds = _textRenderBox.Used;

        TextStyle = settings.GetStyle("popup");
    }

    public TextPopup(GameSettings settings, TextRenderBox textRenderBox)
    {
        _textRenderBox = textRenderBox;

        TextStyle = settings.GetStyle("popup");
    }

    public StyleData TextStyle { get; set; }

    public Dimensions ContentDimensions => _textRenderBox.Bounds.Dimensions;

    public void RenderContent(Rectangle bounds, IRenderer renderer)
    {
        _textRenderBox.Bounds = bounds;
        _textRenderBox.Render(renderer, TextStyle, false);
    }
}
