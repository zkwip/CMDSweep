using CMDSweep.Data;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Menus.MenuItems;

class NumberRangeMenuItem : OptionMenuItem<int>
{
    public readonly int Min;
    public readonly int Max;

    public NumberRangeMenuItem(string title, int min, int max, GameSettings settings) : base(title, Range(min, max), x => x.ToString(), settings)
    {
        Min = min;
        Max = max;
    }

    static List<int> Range(int min, int max)
    {
        List<int> res = new();
        for (int i = min; i <= max; i++) res.Add(i);
        return res;
    }

    internal override bool HandleItemActions(InputAction ia)
    {
        switch (ia)
        {
            case InputAction.Right:
                Index++;
                return true;
            case InputAction.Left:
                Index--;
                return true;
            case InputAction.Clear:
                return TryBackspace();

            case InputAction.One: return TryAdd(1);
            case InputAction.Two: return TryAdd(2);
            case InputAction.Three: return TryAdd(3);
            case InputAction.Four: return TryAdd(4);
            case InputAction.Five: return TryAdd(5);
            case InputAction.Six: return TryAdd(6);
            case InputAction.Seven: return TryAdd(7);
            case InputAction.Eight: return TryAdd(8);
            case InputAction.Nine: return TryAdd(9);
            case InputAction.Zero: return TryAdd(0);
        }

        return false;
    }

    private bool TryBackspace()
    {
        int num = SelectedOption;
        int newnum = num / 10;
        if (!Select(newnum)) return Select(Min);
        return true;
    }

    private bool TryAdd(int digit)
    {
        int num = SelectedOption;
        int newnum = num * 10 + digit;

        if (!Select(newnum))
        {
            //Should remove the first digit?
            newnum %= (int)Math.Pow(10, Math.Floor(Math.Log10(newnum)));
            if (!Select(newnum)) return Select(Max);
        }
        return true;
    }
}
