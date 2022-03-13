﻿using System;
using System.Collections.Generic;

namespace CMDSweep;

static class Text
{
    internal static List<string> WrapText(string text, int horRoom)
    {
        List<String> lines = new(text.Split('\n'));
        int broadest = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            // Wrapping
            if (lines[i].Length > horRoom)
            {
                string line = lines[i];

                int breakpoint = horRoom;
                for (int j = 0; j < horRoom; j++) if (line[j] == ' ') breakpoint = j;

                lines.RemoveAt(i);
                lines.Insert(i, line[..breakpoint]);
                lines.Insert(i + 1, line[..(breakpoint + 1)]);
            }

            broadest = Math.Max(broadest, lines[i].Length);
        }
        return lines;
    }
}

class TextBox
{
    internal string Text;
    internal Rectangle Bounds;
    internal int LineSpacing = 1;

    internal int VerticalScroll = 0;
    internal int HorizontalScroll = 0;

    internal Overflow HorizontalFlow = Overflow.Wrap;
    internal Overflow VerticalFlow = Overflow.Overflow;
    internal VerticalAlignment VerticalAlign = VerticalAlignment.Top;
    internal HorzontalAlignment HorizontalAlign = HorzontalAlignment.Left;

    internal Rectangle TextArea => new(Bounds.Left, Bounds.Top, LongestLineWidth, RenderLineCount);
    public TextBox(string text, Rectangle bounds)
    {
        Text = text;
        Bounds = bounds;
    }

    public TextBox Clone() => new TextBox(Text, Bounds.Clone())
    {
        LineSpacing = LineSpacing,
        VerticalScroll = VerticalScroll,
        HorizontalScroll = HorizontalScroll,
        HorizontalAlign = HorizontalAlign,
        VerticalAlign = VerticalAlign,
        HorizontalFlow = HorizontalFlow,
        VerticalFlow = VerticalFlow
    };

    internal int ScrollUp()
    {
        if (VerticalScroll == 0) VerticalScroll = LowestScroll;
        else VerticalScroll--;
        return VerticalScroll;
    }

    internal int ScrollDown()
    {
        if (VerticalScroll >= LowestScroll) VerticalScroll = 0;
        else VerticalScroll++;
        return VerticalScroll;
    }

    internal int Lines => GetLines().Count;
    internal int LongestLineWidth => Functions.Apply(0, GetLines(), Math.Max, x => x.Length);
    internal int MaxLineCount => Bounds.Height / LineSpacing;
    internal int RenderLineCount => Math.Min(MaxLineCount, Lines);
    internal int LowestScroll => Lines - MaxLineCount;

    internal List<string> GetLines()
    {
        List<String> lines = new(Text.Split('\n'));
        List<String> res = new();

        // Wrapping
        foreach (string line in lines)
        {
            if (res.Count >= MaxLineCount && VerticalFlow == Overflow.Hidden) break;
            else if (line.Length <= Bounds.Width || HorizontalFlow == Overflow.Overflow || HorizontalFlow == Overflow.Scroll) 
                res.Add(line);
            else if (HorizontalFlow == Overflow.Hidden) 
                res.Add(line[..Bounds.Width]);
            else if (HorizontalFlow == Overflow.Wrap)
            {
                string linepart = line;
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
                        linepart = linepart[(splitpoint+1)..];
                    }

                    if (res.Count >= MaxLineCount && VerticalFlow == Overflow.Hidden) break;
                }
                if (linepart.Trim().Length > 0) res.Add(linepart);
            }
        }
        return res;
    }



    internal void Render(IRenderer renderer, StyleData style, bool clear)
    {
        if (clear) renderer.ClearScreen(style, Bounds);

        List<string> lines = GetLines();
        for (int i = 0; i < MaxLineCount; i++)
        {
            int line = i + VerticalScroll;
            if (line >= lines.Count) break;

            int row = Bounds.Top + i * LineSpacing;
            int offset = (Bounds.Width - lines[line].Length) * (int)HorizontalAlign / 2;

            renderer.PrintAtTile(new(Bounds.Left + offset ,row), style, lines[line]);
        }
    }

    private int FindLineSplitPoint(string line)
    {
        int res = 0;
        for (int i = 0; i < Bounds.Width; i++) if (line[i] == ' ') res = i;
        return res;
    }

}

enum Overflow
{
    Wrap,
    Scroll,
    Hidden,
    Overflow
}

enum HorzontalAlignment
{
    Left = 0,
    Center = 1,
    Right = 2,
}

enum VerticalAlignment
{
    Top,
    Middle,
    Bottom,
}