using System;

namespace CMDSweep
{
    class WinCMDRenderer : IRenderer
    {
        public WinCMDRenderer() { }
        public Rectangle Bounds => new(0, 0, Console.WindowWidth, Console.WindowHeight);
        
        public bool ClearScreen(StyleData data)
        {
            SetConsoleStyle(data);
            HideCursor();
            Console.Clear();
            return true;
        }
        
        public bool ClearScreen(StyleData data, Rectangle rec)
        {
            SetConsoleStyle(data);

            rec = rec.Intersect(Bounds);

            for (int row = rec.Top; row < rec.Bottom; row++) PrintAtTile(new Point(row, rec.Left), data, "".PadLeft(rec.Width));
            
            HideCursor();
            return true;
        }

        private void SetConsoleStyle(StyleData data)
        {
            Console.ForegroundColor = data.Foreground;
            Console.BackgroundColor = data.Background;
        }

        public bool SetCursor(Point p)
        {
            if (!Bounds.Contains(p)) return false;
            Console.SetCursorPosition(p.X, p.Y);
            return true;
        }

        public void HideCursor()
        {
            SetCursor(Bounds.TopLeft);
            Console.CursorVisible = false;
        }

        public void HideCursor(StyleData data) => HideCursor(data.Background);
        public void HideCursor(ConsoleColor c)
        {
            SetConsoleStyle(new StyleData(c, c));
            HideCursor();
        }

        public bool PrintAtTile(Point p, StyleData data, string s)
        {
            if (!Bounds.Contains(p)) return false;
            if (Bounds.Right - p.X < s.Length) s = s.Substring(0, Bounds.Right - p.X);

            SetCursor(p);
            SetConsoleStyle(data);
            Console.Write(s);

            return true; 
        }

        public void SetTitle(string s)
        {
            Console.Title = s;
        }
    }
}
