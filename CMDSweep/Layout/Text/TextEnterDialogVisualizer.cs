using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Rendering;

namespace CMDSweep.Layout.Text;

internal class TextEnterDialogVisualizer : IChangeableTypeVisualizer<TextEnterDialog, Rectangle>
{
    private readonly TextRenderBoxVisualizer _messageVisualizer;
    private readonly TextRenderBoxVisualizer _inputVisualizer;

    public TextEnterDialogVisualizer(IRenderer renderer, StyleData popupStyle, StyleData textEnterStyle)
    {
        _messageVisualizer = new TextRenderBoxVisualizer(renderer, popupStyle);
        _inputVisualizer = new TextRenderBoxVisualizer(renderer, textEnterStyle);
    }

    public void Visualize(TextEnterDialog state, Rectangle context)
    {
        Dimensions topDims = state._messageBox.ContentDimensions;
        Dimensions bottomDims = state._messageBox.ContentDimensions;

        Rectangle topRect = new(context.TopLeft, topDims);
        Rectangle bottomRect = new(context.TopLeft.Shift(0, topDims.Height + 1), bottomDims);

        _messageVisualizer.Visualize(state._messageBox, topRect);
        _inputVisualizer.Visualize(state._inputBox, bottomRect);
    }

    public void VisualizeChanges(TextEnterDialog state, Rectangle context, TextEnterDialog oldState, Rectangle oldContext)
    {
        if (context != oldContext)
        {
            Visualize(state, context);
        }

        Dimensions topDims = state._messageBox.ContentDimensions;
        Dimensions bottomDims = state._messageBox.ContentDimensions;

        Rectangle topRect = new(context.TopLeft, topDims);
        Rectangle bottomRect = new(context.TopLeft.Shift(0, topDims.Height + 1), bottomDims);

        _messageVisualizer.VisualizeChanges(state._messageBox, topRect, oldState._messageBox, topRect);
        _inputVisualizer.VisualizeChanges(state._inputBox, bottomRect, oldState._inputBox, bottomRect);

    }
}
