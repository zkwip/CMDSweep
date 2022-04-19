using CMDSweep.Rendering;
using System;

namespace CMDSweep.Views.Menus;

abstract class MenuItem
{
    internal string Title;
    internal bool Focusable = true;
    internal MenuList Parent;

    public event EventHandler ValueChanged;
    internal abstract void RenderItemExtras(int row, MenuVisualizer mv, bool focus);
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

    internal void RenderItem(int row, MenuVisualizer mv, bool focus)
    {
        StyleData styl = focus ? mv.FocusTitleStyle : mv.MenuTextStyle;

        mv.Renderer.ClearScreen(mv.MenuTextStyle, mv.TableGrid.Row("items", row));

        mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("labels", 0, "items", row), styl, Title);
        RenderItemExtras(row, mv, focus);
    }

}
