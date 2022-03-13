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

class TextRenderBox
{
    internal string Text;
    internal Rectangle Bounds;
    internal int LineSpacing = 1;

    internal int VerticalScroll = 0;
    internal int HorizontalScroll = 0;

    internal bool HorizontalOverflow = false;
    internal bool VerticalOverflow = true;
    internal bool Wrap = true;

    internal VerticalAlignment VerticalAlign = VerticalAlignment.Top;
    internal HorzontalAlignment HorizontalAlign = HorzontalAlignment.Left;

    internal Rectangle Used => new(Bounds.Left, Bounds.Top, LongestLineWidth, RenderLineCount);

    public TextRenderBox()
    {
        Text = "";
        Bounds = Rectangle.Zero;
    }
    public TextRenderBox(string text, Rectangle bounds)
    {
        Text = text;
        Bounds = bounds;
    }

    public TextRenderBox Clone() => new(Text, Bounds.Clone())
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

    internal int ScrollLeft()
    {
        if (HorizontalScroll == 0) HorizontalScroll = RightmostScroll;
        else HorizontalScroll--;
        return HorizontalScroll;
    }

    internal int ScrollRight()
    {
        if (HorizontalScroll >= RightmostScroll) HorizontalScroll = 0;
        else HorizontalScroll++;
        return HorizontalScroll;
    }

    Rectangle CharTable => new(0,0, Bounds.Width, Lines);
    Rectangle CharTableViewPort => new Rectangle(new LinearRange(HorizontalScroll, Bounds.Width), new LinearRange(VerticalScroll,MaxLineCount));

    internal int Lines => GetLines().Count;
    internal int LongestLineWidth => Functions.Apply(0, GetLines(), Math.Max, x => x.Length);
    internal int MaxLineCount => Bounds.Height / LineSpacing;
    internal int RenderLineCount => Math.Min(MaxLineCount, Lines);
    internal int LowestScroll => Math.Max(0, Lines - MaxLineCount);
    internal int RightmostScroll => Math.Max(0, LongestLineWidth - Bounds.Width);

    internal List<string> GetLines()
    {
        List<String> lines = new(Text.Split('\n'));
        List<String> res = new();

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


    internal void Render(IRenderer renderer, StyleData style, bool clear)
    {
        if (clear) renderer.ClearScreen(style, Bounds);

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
                if (text.Length > cut) text = text[(cut)..];
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
                renderer.PrintAtTile(new(render_x, render_y), style, text);
        }
    }

    private int FindLineSplitPoint(string line)
    {
        int res = 0;
        for (int i = 0; i < Bounds.Width; i++) if (line[i] == ' ') res = i;
        return res;
    }

}

class TextEnterField : TextRenderBox
{
    internal bool AllowEnter = false;

    public IRenderer Renderer;
    public StyleData Style;
    internal bool Active { get; private set; }
    public Controller Controller { get; }

    internal TextEnterField(Controller c, Rectangle bounds, IRenderer r, StyleData sd) : base("", bounds)
    {
        Renderer = r;
        Style = sd;
        Active = false;
        Controller = c;
        Wrap = false;
    }

    public void Render() => Render(Renderer, Style, true);
    public ConsoleKeyInfo Activate()
    {
        Active = true;
        ConsoleKeyInfo info;

        Render();
        while (true)
        {
            info = Console.ReadKey(true);
            char c = info.KeyChar;

            if (c == '\0') break;
            if (info.Key == ConsoleKey.Enter && !AllowEnter) break;
            if (info.Key == ConsoleKey.Enter && AllowEnter) c='\n';
            if (info.Key == ConsoleKey.Escape) break;

            if (info.Key == ConsoleKey.Backspace)
            {
                if (Text.Length > 0) Text = Text[..(Text.Length - 1)];
                else Console.Beep();
            }
            else
            {
                Text += c;
            }
            HorizontalScroll = RightmostScroll;
            Render();
        }

        Active = false;
        return info;
    }
}

enum HorzontalAlignment
{
    Left = 0,
    Center = 1,
    Right = 2,
}

enum VerticalAlignment
{
    Top = 0,
    Middle = 1,
    Bottom = 2,
}