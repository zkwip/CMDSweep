using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Layout.Text;

internal class TextRenderBoxVisualizer : IChangeableTypeVisualizer<TextRenderBox, Rectangle>
{
    private readonly IRenderer _renderer;
    private readonly StyleData _textData;

    public TextRenderBoxVisualizer(IRenderer renderer, StyleData textStyle)
    {
        _renderer = renderer;
        _textData = textStyle;
    }

    public void Visualize(TextRenderBox box, Rectangle bounds)
    {
        RenderLines(_renderer, box, bounds, 0, box.MaxLineCount, true);
    }

    private void RenderLines(IRenderer renderer, TextRenderBox box, Rectangle bounds, int start, int end, bool clear)
    {
        if (clear)
            renderer.ClearScreen(_textData, LineBounds(start, end, bounds, box));

        List<string> lines = box.GetLines();
        int inner_width = Math.Max(bounds.Width, box.LongestLineWidth);

        for (int i = start; i < end; i++)
        {
            // vertical alignment
            SingleLine(renderer, box, lines, inner_width, i, bounds);
        }
    }

    private void RenderScroll(IRenderer renderer, TextRenderBox box, Rectangle bounds, int distance)
    {
        if (distance == 0)
            return;

        if (0 < distance && distance < box.MaxLineCount)
        {
            RenderBufferCopyTask task = new(LineBounds(distance, box.MaxLineCount, bounds, box), LineBounds(0, box.MaxLineCount - distance, bounds, box));
            renderer.CopyArea(task);

            RenderLines(renderer, box, bounds, box.MaxLineCount - distance, box.MaxLineCount, true);
            return;
        }

        distance = -distance;

        if (0 < distance && distance < box.MaxLineCount)
        {
            RenderBufferCopyTask task = new(LineBounds(0, box.MaxLineCount - distance, bounds, box), LineBounds(distance, box.MaxLineCount, bounds, box));
            renderer.CopyArea(task);

            RenderLines(renderer, box, bounds, 0, distance, true);
            return;
        }

        Visualize(box, bounds);

    }

    private static Rectangle LineBounds(int start, int end, Rectangle bounds, TextRenderBox box)
    {
        int start_y = bounds.Top + start * box.LineSpacing;
        int end_y = bounds.Top + end * box.LineSpacing;

        return new Rectangle(bounds.HorizontalRange, new LinearRange(start_y, end_y - start_y));
    }

    private bool SingleLine(IRenderer renderer, TextRenderBox box, List<string> lines, int inner_width, int index, Rectangle bounds)
    {
        int line = index + box.VerticalScroll;

        if (line >= lines.Count)
            return false;

        string text = lines[line];

        // horizontal alignment of the first char of the string to relative to the other text
        int indent = (inner_width - text.Length) * (int)box.HorizontalAlign / 2;

        int render_x = bounds.Left - box.HorizontalScroll + indent;
        int render_y = bounds.Top + index * box.LineSpacing;

        int clipping_x = 0;
        if (!box.HorizontalOverflow)
            clipping_x = Math.Max(0, bounds.Left);

        int cut = clipping_x - render_x;

        // trim the start if it the line extends back before the start of the box, if needed
        if (cut > 0)
        {
            if (text.Length <= cut)
                return true;

            text = text[cut..];
            render_x += cut;
        }

        // trim the end if needed
        if (!box.HorizontalOverflow)
        {
            int end = bounds.Right - render_x;
            if (end < 0)
                return true;

            if (text.Length > end)
                text = text[..end];
        }

        if (text.Length > 0)
            renderer.PrintAtTile(new(render_x, render_y), _textData, text);

        return true;
    }

    public void VisualizeChanges(TextRenderBox box, Rectangle bounds, TextRenderBox oldBox, Rectangle oldBounds)
    {
        Visualize(box, bounds); // Todo: make it actually changeable?
    }
}
