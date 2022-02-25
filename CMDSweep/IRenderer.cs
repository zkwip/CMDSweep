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

        public event EventHandler BoundsChanged;

        void CopyArea(Rectangle oldArea, Rectangle newArea);
    }

    public class BoundsChangedEventArgs : EventArgs
    {
        public Rectangle NewBounds { get; }
        public Rectangle OldBounds { get; }

        public BoundsChangedEventArgs(Rectangle o, Rectangle n) { NewBounds = n; OldBounds = o;}
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
