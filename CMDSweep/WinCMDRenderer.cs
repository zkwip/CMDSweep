using System;

namespace CMDSweep
{
    class WinCMDRenderer : IRenderer
    {
        public WinCMDRenderer()
        {

        }

        public Bounds Bounds
        {
            get { return new Bounds(Console.WindowWidth, Console.WindowHeight); }
        }

        public void ClearScreen(StyleData data)
        {
            SetConsoleStyle(data);
            HideCursor();
            Console.Clear();
        }

        public void ClearScreen(StyleData data, int row) => ClearScreen(data, row, 0, Bounds.Width);
        public void ClearScreen(StyleData data, int row, int col, int width) => ClearScreen(data, row, col, width, 1);
        public void ClearScreen(StyleData data, int row, int col, int width, int height)
        {
            SetConsoleStyle(data);
            for (int r = row; r < row + height; r++)
                PrintAtTile(r, col, data, "".PadLeft(width));
            HideCursor();
        }

        private void SetConsoleStyle(StyleData data)
        {
            Console.ForegroundColor = data.Foreground;
            Console.BackgroundColor = data.Background;
        }

        public void SetCursor(int row, int col)
        {
            try
            {
                Console.SetCursorPosition(col, row);
            }
            catch (ArgumentOutOfRangeException e)
            {
                //todo
            }
        }

        public void HideCursor()
        {
            SetCursor(0, 0);
            Console.CursorVisible = false;
        }

        public void HideCursor(StyleData data)
        {
            SetConsoleStyle(data);
            HideCursor();
        }

        public void PrintAtTile(int row, int col, StyleData data, string s)
        {
            SetCursor(row, col);
            SetConsoleStyle(data);
            Console.Write(s);
        }

        public void SetTitle(string s)
        {
            Console.Title = s;
        }
    }
}
