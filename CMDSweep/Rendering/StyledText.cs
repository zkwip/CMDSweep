namespace CMDSweep.Rendering;

internal record struct StyledText
{
    public readonly string Text;
    public readonly StyleData Style;

    public StyledText(string text, StyleData style)
    {
        Text = text;
        Style = style;
    }
}
