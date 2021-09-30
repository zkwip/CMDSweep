using System;

namespace CMDSweep
{
    interface IRenderer
    {
        void SetTile(int row, int col, TileData data);
        void SetCursor(int row, int col);
        void ClearScreen(TileData data);
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


    public struct TileData
    {
        public TileData(ConsoleColor fg, ConsoleColor bg, char c, bool h) { Foreground = fg; Background = bg; Symbol = c; Highlight = h; }

        public readonly ConsoleColor Foreground;
        public readonly ConsoleColor Background;
        public readonly char Symbol;
        public readonly bool Highlight;
    }
}
