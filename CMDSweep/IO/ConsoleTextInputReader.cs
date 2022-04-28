using System;

namespace CMDSweep.IO;

internal class ConsoleTextInputReader
{
    public static (string text, bool done) HandleTypingKeyPress(bool allowLineBreak, string text)
    {
        ConsoleKeyInfo info;

        info = Console.ReadKey(true);
        char c = info.KeyChar;

        if (c == '\0') 
            return (text, true);

        if (info.Key == ConsoleKey.Enter && !allowLineBreak)
            return (text, true);

        if (info.Key == ConsoleKey.Enter) 
            return (text + '\n',false);
            
        if (info.Key == ConsoleKey.Escape)
            return (text, true);

        if (info.Key == ConsoleKey.Backspace)
        {
            if (text.Length > 0)
                return (text[..(text.Length - 1)], true);
            
            Console.Beep();
            return (text, true);
        }
        
        return (text + c, false);
    }
}
