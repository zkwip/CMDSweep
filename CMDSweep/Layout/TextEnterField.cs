using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System;

namespace CMDSweep.Layout;

class TextEnterField : TextRenderBox
{
    private StyleData _styleData;

    public bool AllowEnter = false;
    public bool Active { get; private set; }

    internal TextEnterField(Rectangle bounds, StyleData sd) : base("", bounds, sd)
    {
        _styleData = sd;
        Active = false;
        Wrap = false;
    }
    
    
    public ConsoleKeyInfo Activate(IRenderer renderer)
    {
        Active = true;
        ConsoleKeyInfo info;

        Render(renderer, true);
        while (true)
        {
            info = Console.ReadKey(true);
            char c = info.KeyChar;

            if (c == '\0') break;
            if (info.Key == ConsoleKey.Enter && !AllowEnter) break;
            if (info.Key == ConsoleKey.Enter && AllowEnter) c = '\n';
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
            Render(renderer, true);
        }

        Active = false;
        return info;
    }
}
