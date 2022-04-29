using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Layout.Text;

class TextRenderBox : IBounded, IRenderState, IPlaceable
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

    public Rectangle Used => new(Bounds.Left, Bounds.Top, LongestLineWidth, RenderLineCount);

    public Dimensions Dimensions { get => Bounds.Dimensions; set => Bounds = new Rectangle(Bounds.TopLeft, value); }

    public TextRenderBox(int id = 0)
    {
        Text = "";
        Bounds = Rectangle.Zero;
        _id = id;
    }

    public TextRenderBox(string text, Rectangle bounds, int id = 0)
    {
        Text = text;
        Bounds = bounds;
        _id = id;
    }

    public int Id => _id;

    public TextRenderBox Clone() => new(Text, Bounds, _id + 1)
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

    public Dimensions ContentDimensions => Bounds.Dimensions;

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
