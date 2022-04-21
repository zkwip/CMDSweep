using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Layout;

class TextRenderBox : IBounded
{
    internal string Text;
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

    public TextRenderBox(StyleData styleData)
    {
        Text = "";
        Bounds = Rectangle.Zero;
        StyleData = styleData;
    }
    public TextRenderBox(string text, Rectangle bounds, StyleData styleData)
    {
        Text = text;
        Bounds = bounds;
        StyleData = styleData;
    }

    public TextRenderBox Clone() => new(Text, Bounds, StyleData)
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
        if (VerticalScroll == 0) VerticalScroll = LowestScroll;
        else VerticalScroll--;
        return VerticalScroll;
    }

    public int ScrollDown()
    {
        if (VerticalScroll >= LowestScroll) VerticalScroll = 0;
        else VerticalScroll++;
        return VerticalScroll;
    }

    public int ScrollLeft()
    {
        if (HorizontalScroll == 0) HorizontalScroll = RightmostScroll;
        else HorizontalScroll--;
        return HorizontalScroll;
    }

    public int ScrollRight()
    {
        if (HorizontalScroll >= RightmostScroll) HorizontalScroll = 0;
        else HorizontalScroll++;
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
        List<string> res = new();

        // Wrapping
        foreach (string line in lines)
        {
            string linepart = line;
            if (linepart.Length == 0 || !Wrap)
            {
                res.Add(linepart);
                continue;
            }
            while (linepart.Length > Bounds.Width)
            {
                int splitpoint = FindLineSplitPoint(linepart);

                if (splitpoint == 0)
                {
                    res.Add(linepart[..Bounds.Width]);
                    linepart = linepart[Bounds.Width..];
                }
                else
                {
                    res.Add(linepart[..splitpoint]);
                    linepart = linepart[(splitpoint + 1)..];
                }
            }
            if (linepart.Length > 0) res.Add(linepart);
        }
        return res;
    }

    internal void Render(IRenderer renderer, bool clear)
    {
        if (clear) renderer.ClearScreen(StyleData, Bounds);

        List<string> lines = GetLines();
        int inner_width = Math.Max(Bounds.Width, LongestLineWidth);

        for (int i = 0; i < MaxLineCount; i++)
        {
            // vertical alignment
            int line = i + VerticalScroll;
            if (line >= lines.Count) break;
            int render_y = Bounds.Top + i * LineSpacing;

            string text = lines[line];

            // horizontal alignment of the first char of the string to relative to the other text
            int indent = (inner_width - text.Length) * (int)HorizontalAlign / 2;

            // absolute horizontal alignment of the first char of the string
            int render_x = Bounds.Left - HorizontalScroll + indent;

            // decide from where the string needs to be printed
            int clipping_x = 0;
            if (!HorizontalOverflow) clipping_x = Math.Max(0, Bounds.Left);

            int cut = clipping_x - render_x;

            // trim the start if it the line extends back before the start of the box, if needed
            if (cut > 0)
            {
                if (text.Length > cut) text = text[cut..];
                else continue;
                render_x += cut;
            }

            // trim the end if needed
            if (!HorizontalOverflow)
            {
                int end = Bounds.Right - render_x;
                if (end < 0) continue;
                else if (text.Length > end) text = text[..end];
            }

            if (text.Length > 0)
                renderer.PrintAtTile(new(render_x, render_y), StyleData, text);
        }
    }

    private int FindLineSplitPoint(string line)
    {
        int res = 0;
        for (int i = 0; i < Bounds.Width; i++) if (line[i] == ' ') res = i;
        return res;
    }

}
