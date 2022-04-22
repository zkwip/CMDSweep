using CMDSweep.Layout;
using CMDSweep.Rendering;
using System;

namespace CMDSweep.Views.Menus;

abstract class MenuItem
{
    internal string Title;
    internal bool Focusable = true;
    internal MenuList Parent;

    public event EventHandler ValueChanged;

    internal abstract void RenderItemExtras(IRenderer renderer, int row, TableGrid tableGrid, bool focus);

    internal abstract bool HandleItemActions(InputAction ia);

    internal virtual void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);

    internal void BindParent(MenuList menuList) => Parent = menuList;

    internal MenuItem(string title)
    {
        Title = title;
    }

    internal bool HandleMenuAction(InputAction ia)
    {
        switch (ia)
        {
            case InputAction.Up:
                Parent.FocusPrevious();
                return true;
            case InputAction.Down:
                Parent.FocusNext();
                return true;
            default:
                return HandleItemActions(ia);
        }
    }

    public static string CenterAlign(string text, int length)
    {
        int offset = (length - text.Length) / 2;
        text += "".PadRight(offset);
        return text.PadLeft(length);
    }

}
