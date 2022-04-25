using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Menus;

class MenuList
{
    public MenuList ParentMenu;
    public MenuController Controller;

    public List<MenuItem> Items { get; private set; }
    public string Title { get; private set; }
    public int FocusIndex { get; private set; }
    public MenuItem FocusedItem { get => Items[FocusIndex]; }
    public int Length { get => Items.Count; }

    public int FocusNext()
    {
        if (Length == 0) return FocusIndex;
        for (int i = 0; i < 100; i++)
        {
            FocusIndex++;
            if (FocusIndex >= Length) FocusIndex -= Length;
            if (FocusedItem.Focusable) return FocusIndex;
        }
        return FocusIndex;
    }

    public int FocusPrevious()
    {
        if (Length == 0) return FocusIndex;
        for (int i = 0; i < 100; i++)
        {
            FocusIndex--;
            if (FocusIndex < 0) FocusIndex += Length;
            if (FocusedItem.Focusable) return FocusIndex;
        }
        return FocusIndex;
    }

    public MenuList(string title, MenuController mc)
    {
        Controller = mc;
        Items = new List<MenuItem>();
        Title = title;
        FocusIndex = 0;
    }

    public void Add(MenuItem item)
    {
        Items.Add(item);
        item.BindParent(this);
    }

    internal bool HandleInput(InputAction ia)
    {
        if (ia == InputAction.Quit)
        {
            if (ParentMenu == null) return false;
            else Controller.OpenMenu(ParentMenu);
        }

        if (FocusedItem != null) FocusedItem.HandleMenuAction(ia);
        return true;
    }

    internal int FixScroll(int depth, int maxRows)
    {
        int space = 1;
        int end = Length - maxRows;
        while (depth > 0 && depth > FocusIndex - space) depth--;
        while (depth < end && depth <= FocusIndex + space - maxRows) depth++;
        return depth;
    }

    internal void AddButton(string text, Action action)
    {
        ButtonMenuItem mb = new(text);
        mb.ValueChanged += (i, o) => action();
        Add(mb);
    }
}