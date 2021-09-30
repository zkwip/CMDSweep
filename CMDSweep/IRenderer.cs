using System;

namespace CMDSweep
{
    interface IRenderer
    {
        void PrintAtTile(int row, int col, StyleData data, string s);
        void SetCursor(int row, int col);
        void ClearScreen(StyleData data);
        void HideCursor();
        void SetTitle(string s);
        Bounds Bounds { get; }
    }

    public struct Bounds
    {
        public Bounds(int w, int h) { Width = w; Height = h; }
        public readonly int Width;
        public readonly int Height;

        public static bool operator ==(Bounds b1, Bounds b2) => b1.Width == b2.Width && b1.Height == b2.Height;
        public static bool operator !=(Bounds b1, Bounds b2) => b1.Width != b2.Width || b1.Height != b2.Height;
    }


    public struct StyleData
    {
        public StyleData(ConsoleColor fg, ConsoleColor bg, char c, bool h) { Foreground = fg; Background = bg; Highlight = h; }

        public readonly ConsoleColor Foreground;
        public readonly ConsoleColor Background;
        public readonly bool Highlight;
    }
}
