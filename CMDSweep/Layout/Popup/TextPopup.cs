using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;

namespace CMDSweep.Layout.Popup;

internal class TextPopup : IPopup
{
    private TextRenderBox _textRenderBox;
    private StyleData _textStyle;

    public TextPopup(GameSettings settings, string text, Dimensions maxDimensions)
    {
        _textStyle = settings.GetStyle("popup");
        _textRenderBox = new TextRenderBox(_textStyle);
        _textRenderBox.Bounds = new Rectangle(0, 0, maxDimensions.Width, maxDimensions.Height);
        _textRenderBox.Text = text;
        _textRenderBox.Bounds = _textRenderBox.Used;
    }

    public TextPopup(TextRenderBox textRenderBox)
    {
        _textRenderBox = textRenderBox;
        _textStyle = _textRenderBox.StyleData;
    }

    public int Id => 0;

    public Dimensions ContentDimensions => _textRenderBox.Bounds.Dimensions;

    public void RenderContent(Rectangle bounds, IRenderer renderer)
    {
        _textRenderBox.Bounds = bounds;
        _textRenderBox.Render(renderer, false);
    }
}
