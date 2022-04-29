using System;
using System.Collections.Generic;

namespace CMDSweep.IO;

internal class ConsoleInputReader
{
    public static (string text, bool done, InputAction lastPress) HandleTypingKeyPress(bool allowLineBreak, string text)
    {
        ConsoleKeyInfo info;

        info = Console.ReadKey(true);
        char c = info.KeyChar;
        InputAction action = ParseAction(info);

        if (c == '\0')
            return (text, true, action);

        if (info.Key == ConsoleKey.Enter && !allowLineBreak)
            return (text, true, action);

        if (info.Key == ConsoleKey.Enter)
            return (text + '\n', false, action);

        if (info.Key == ConsoleKey.Escape)
            return (text, true, action);

        if (info.Key == ConsoleKey.Backspace)
        {
            if (text.Length > 0)
                return (text[..(text.Length - 1)], false, action);

            Console.Beep();
            return (text, false, action);
        }

        return (text + c, false, action);
    }
    public static InputAction ReadAction() => ParseAction(Console.ReadKey(true));

    public static InputAction ParseAction(ConsoleKeyInfo info)
    {
        ConsoleKey key = info.Key;
        foreach (KeyValuePair<InputAction, List<ConsoleKey>> ctrl in Settings.Controls)
            if (ctrl.Value.Contains(key))
                return ctrl.Key;

        return InputAction.Unknown;
    }
}
