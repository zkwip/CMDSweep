using System;

namespace CMDSweep
{
    public interface IRenderer
    {
        bool PrintAtTile(Point p, StyleData data, string s);
        bool SetCursor(Point p);
        bool ClearScreen(StyleData data);
        bool ClearScreen(StyleData data, Rectangle r);
        bool ClearScreen(StyleData data, int row) => ClearScreen(data, new Rectangle(0, row, Bounds.Width, 0));

        void SetTitle(string s);
        Rectangle Bounds { get; }

        void HideCursor(StyleData styleOutOfBounds);
    }

    public struct StyleData
    {
        public StyleData(ConsoleColor fg, ConsoleColor bg) { Foreground = fg; Background = bg; Highlight = false; }
        public StyleData(ConsoleColor fg, ConsoleColor bg, bool h) { Foreground = fg; Background = bg; Highlight = h; }

        public readonly ConsoleColor Foreground;
        public readonly ConsoleColor Background;
        public readonly bool Highlight;
    }
}
