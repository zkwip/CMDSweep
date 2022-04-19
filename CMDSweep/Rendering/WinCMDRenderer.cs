using CMDSweep.Geometry;
using System;
using System.Runtime.InteropServices;
using System.Timers;

namespace CMDSweep.Rendering;

class WinCMDRenderer : IRenderer
{
    public WinCMDRenderer()
    {
        Timer t = new(50);
        t.Elapsed += ResizeTesterElapsed;
        t.Start();
        lastBounds = Bounds;
    }

    protected virtual void OnBoundsChanged(BoundsChangedEventArgs args)
    {
        BoundsChanged?.Invoke(this, args);
    }

    private void ResizeTesterElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!lastBounds.Equals(Bounds))
        {
            OnBoundsChanged(new(lastBounds, Bounds));
            lastBounds = Bounds;
        }
    }

    public Rectangle Bounds => new(0, 0, Console.WindowWidth, Console.WindowHeight);
    private Rectangle lastBounds;

    public event EventHandler BoundsChanged;

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

        for (int row = rec.Top; row < rec.Bottom; row++)
            PrintAtTile(new Point(rec.Left, row), data, "".PadLeft(rec.Width));

        HideCursor();
        return true;
    }

    public void CopyArea(Rectangle oldArea, Rectangle newArea)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Console.MoveBufferArea(oldArea.Left, oldArea.Top, oldArea.Width, oldArea.Height, newArea.Left, newArea.Top);
        else
            throw new NotImplementedException();
    }

    private static void SetConsoleStyle(StyleData data)
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
        if (Bounds.Right - p.X < s.Length) s = s[..(Bounds.Right - p.X)];
        if (p.X < Bounds.Left)
        {
            s = s[(Bounds.Left - p.X)..];
            p = new Point(Bounds.Left, p.Y);
        }

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
