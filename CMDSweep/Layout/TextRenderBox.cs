using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Layout;

class TextRenderBox : IBounded, IRenderState
{
    internal string Text;
    private int _id;

    public Rectangle Bounds { get; set; }
    public int LineSpacing = 1;

    public int VerticalScroll = 0;
    public int HorizontalScroll = 0;

    public bool HorizontalOverflow = false;
    public bool VerticalOverflow = true;
    public bool Wrap = true;

    public VerticalAlignment VerticalAlign = VerticalAlignment.Top;

    public HorzontalAlignment HorizontalAlign = HorzontalAlignment.Left;

    public StyleData StyleData { get; set; }

    public Rectangle Used => new(Bounds.Left, Bounds.Top, LongestLineWidth, RenderLineCount);

    public TextRenderBox(StyleData styleData, int id=0)
    {
        Text = "";
        Bounds = Rectangle.Zero;
        StyleData = styleData;
        _id = id; 
    }

    public TextRenderBox(string text, Rectangle bounds, StyleData styleData, int id=0)
    {
        Text = text;
        Bounds = bounds;
        StyleData = styleData;
        _id = id;
    }

    public int Id => _id;

    public TextRenderBox Clone() => new(Text, Bounds, StyleData, _id + 1)
    {
        LineSpacing = LineSpacing,
        VerticalScroll = VerticalScroll,
        HorizontalScroll = HorizontalScroll,
        HorizontalAlign = HorizontalAlign,
        VerticalAlign = VerticalAlign,
        HorizontalOverflow = HorizontalOverflow,
        VerticalOverflow = VerticalOverflow,
        Wrap = Wrap,
    };

    public int ScrollUp()
    {
        if (VerticalScroll == 0) 
            VerticalScroll = LowestScroll;
        else 
            VerticalScroll--;

        return VerticalScroll;
    }

    public int ScrollDown()
    {
        if (VerticalScroll >= LowestScroll) 
            VerticalScroll = 0;
        else 
            VerticalScroll++;

        return VerticalScroll;
    }

    public int ScrollLeft()
    {
        if (HorizontalScroll == 0) 
            HorizontalScroll = RightmostScroll;
        else 
            HorizontalScroll--;

        return HorizontalScroll;
    }

    public int ScrollRight()
    {
        if (HorizontalScroll >= RightmostScroll) 
            HorizontalScroll = 0;
        else 
            HorizontalScroll++;

        return HorizontalScroll;
    }

    public int Lines => GetLines().Count;
    
    public int LongestLineWidth => GeometryFunctions.Apply(0, GetLines(), Math.Max, x => x.Length);
    
    public int MaxLineCount => Bounds.Height / LineSpacing;
    
    public int RenderLineCount => Math.Min(MaxLineCount, Lines);
    
    public int LowestScroll => Math.Max(0, Lines - MaxLineCount);
    
    public int RightmostScroll => Math.Max(0, LongestLineWidth - Bounds.Width);

    internal List<string> GetLines()
    {
        List<string> lines = new(Text.Split('\n'));

        if (Wrap)
            return WrappedLines(lines);

        return lines;
    }

    private List<string> WrappedLines(List<string> lines)
    {
        List<string> res = new();

        // Wrapping
        foreach (string line in lines)
        {
            string linepart = line;

            if (linepart.Length == 0 || Bounds.Width <= 0)
            {
                res.Add(linepart);
                continue;
            }

            while (linepart.Length > Bounds.Width)
            {
                int breakpoint = FindLineBreakingPoint(linepart);

                res.Add(linepart[..breakpoint]);
                linepart = linepart[(breakpoint + 1)..].TrimStart();
            }

            if (linepart.Length > 0) res.Add(linepart);
        }

        return res;
    }

    internal void Render(IRenderer renderer, bool clear) => RenderLines(renderer, 0, MaxLineCount, clear);

    private void RenderLines(IRenderer renderer, int start, int end, bool clear)
    {
        if (clear)
            renderer.ClearScreen(StyleData, LineBounds(start, end));

        List<string> lines = GetLines();
        int inner_width = Math.Max(Bounds.Width, LongestLineWidth);

        for (int i = start; i < end; i++)
        {
            // vertical alignment
            SingleLine(renderer, lines, inner_width, i);
        }
    }

    internal void RenderScroll(IRenderer renderer, int distance)
    {
        if (distance == 0)
            return;

        if (0 < distance && distance < MaxLineCount)
        {
            RenderBufferCopyTask task = new(LineBounds(distance, MaxLineCount), LineBounds(0, MaxLineCount - distance));
            renderer.CopyArea(task);

            RenderLines(renderer, MaxLineCount - distance, MaxLineCount, true);
            return;
        }

        distance = -distance;

        if (0 < distance && distance < MaxLineCount)
        {
            RenderBufferCopyTask task = new(LineBounds(0, MaxLineCount - distance), LineBounds(distance, MaxLineCount));
            renderer.CopyArea(task);

            RenderLines(renderer, 0, distance, true);
            return;
        }

        Render(renderer, true);

    }

    private Rectangle LineBounds(int start, int end)
    {
        int start_y = Bounds.Top + start * LineSpacing;
        int end_y = Bounds.Top + end * LineSpacing;

        return new Rectangle(Bounds.HorizontalRange, new LinearRange(start_y, end_y - start_y));
    }

    private bool SingleLine(IRenderer renderer, List<string> lines, int inner_width, int index)
    {
        int line = index + VerticalScroll;
        
        if (line >= lines.Count) 
            return false;

        string text = lines[line];

        // horizontal alignment of the first char of the string to relative to the other text
        int indent = (inner_width - text.Length) * (int)HorizontalAlign / 2;

        int render_x = Bounds.Left - HorizontalScroll + indent;
        int render_y = Bounds.Top + index * LineSpacing;

        int clipping_x = 0;
        if (!HorizontalOverflow) 
            clipping_x = Math.Max(0, Bounds.Left);

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
        if (!HorizontalOverflow)
        {
            int end = Bounds.Right - render_x;
            if (end < 0)
                return true;

            if (text.Length > end) 
                text = text[..end];
        }

        if (text.Length > 0)
            renderer.PrintAtTile(new(render_x, render_y), StyleData, text);

        return true;
    }

    private int FindLineBreakingPoint(string line)
    {
        int res = 0;

        for (int i = 0; i < Bounds.Width; i++)
        {
            if (line[i] == ' ') 
                res = i;
        }

        if (res == 0) 
            return Bounds.Width;

        return res;
    }

}
