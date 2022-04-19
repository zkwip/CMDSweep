using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System;

namespace CMDSweep.Layout;

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
            Render();
        }

        Active = false;
        return info;
    }
}
