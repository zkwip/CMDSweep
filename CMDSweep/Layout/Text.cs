using System;
using System.Collections.Generic;

namespace CMDSweep.Layout;

static class Text
{
    internal static List<string> WrapText(string text, int horRoom)
    {
        List<string> lines = new(text.Split('\n'));
        int broadest = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            // Wrapping
            if (lines[i].Length > horRoom)
            {
                string line = lines[i];

                int breakpoint = horRoom;
                for (int j = 0; j < horRoom; j++) if (line[j] == ' ') breakpoint = j;

                lines.RemoveAt(i);
                lines.Insert(i, line[..breakpoint]);
                lines.Insert(i + 1, line[..(breakpoint + 1)]);
            }

            broadest = Math.Max(broadest, lines[i].Length);
        }
        return lines;
    }

    internal static string PadText(string text, int length, HorzontalAlignment alignment)
    {
        int padding = length - text.Length;
        
        if (padding == 0) 
            return text;

        int offset = (int)alignment * padding / 2;

        if (padding < 0)
            return text.Substring(-offset, length);

        text = "".PadRight(offset) + text;
        return text.PadRight(length);
       
    }
}

enum HorzontalAlignment
{
    Left = 0,
    Center = 1,
    Right = 2,
}

enum VerticalAlignment
{
    Top = 0,
    Middle = 1,
    Bottom = 2,
}