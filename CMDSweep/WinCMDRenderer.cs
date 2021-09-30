using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDSweep
{
    class WinCMDRenderer : IRenderer
    {
        public WinCMDRenderer()
        {

        }

        public Bounds Bounds
        {
            get {  return new Bounds(Console.WindowWidth, Console.WindowHeight);}
        }

        public void ClearScreen(StyleData data)
        {
            SendDataToConsole(data);
            Console.Clear();
        }

        private void SendDataToConsole(StyleData data)
        {
            Console.ForegroundColor = data.Foreground;
            Console.BackgroundColor = data.Background;
        }

        public void SetCursor(int row, int col)
        {
            Console.SetCursorPosition(col, row);
        }

        public void HideCursor()
        {
            SetCursor(0, 0);
            Console.CursorVisible = false;
        }

        public void PrintAtTile(int row, int col, StyleData data, string s)
        {
            SetCursor(row, col);
            SendDataToConsole(data);
            Console.Write(s);
        }

        public void SetTitle(string s)
        {
            Console.Title = s;
        }
    }
}
