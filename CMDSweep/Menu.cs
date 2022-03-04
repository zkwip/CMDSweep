using System;
using System.Collections.Generic;

namespace CMDSweep;

public class MenuVisualizer
{
    internal TableGrid TableGrid;
    internal int scrollDepth = 0;
    internal int maxRows = 4;
    internal GameApp Game;
    internal IRenderer Renderer => Game.Renderer;


    public StyleData MenuTextStyle => Game.Settings.GetStyle("menu");
    public StyleData FocusBoxStyle => Game.Settings.GetStyle("menu-highlight-box");
    public StyleData FocusTitleStyle => Game.Settings.GetStyle("menu-highlight-title");
    public MenuList CurrentList => Game.currentMenuList;
    internal Dictionary<string, ConsoleColor> Colors => Game.Settings.Colors;

    public MenuVisualizer(GameApp g) => Game = g;

    public bool Visualize(RefreshMode mode)
    {
        Renderer.ClearScreen(MenuTextStyle);
        Renderer.HideCursor(MenuTextStyle);
        Renderer.SetTitle(Game.Settings.Texts["menu-title"]);

        if (CurrentList == null) return false;
        VisualizeList(CurrentList);

        return true;
    }

    private void UpdateMeasurements()
    {
        Dictionary<string, int> dims = Game.Settings.Dimensions;
        TableGrid tg = this.TableGrid = new();
        maxRows = dims["menu-rows"];

        // Columns
        tg.AddColumn(dims["menu-indent"], 0, "prefix");
        tg.AddColumn(dims["menu-col1-width"], 0, "labels");
        tg.AddColumn(dims["menu-box-padding"], 0);
        tg.AddColumn(dims["menu-box-padding"], 0, "pre-options");
        tg.AddColumn(dims["menu-col2-width"], 0, "options");
        tg.AddColumn(dims["menu-box-padding"], 0, "post-options");

        // Rows
        tg.AddRow(dims["menu-title-space"], 0, "title");
        tg.AddRow(dims["menu-row-scale"], 0, "items", maxRows);

        tg.FitAround();
        tg.CenterOn(Renderer.Bounds.Center);
    }

    private void VisualizeList(MenuList list)
    {
        UpdateMeasurements();
        scrollDepth = list.FixScroll(scrollDepth, maxRows);
        RenderTitle(list.Title);
        for (int i = 0; i + scrollDepth < list.Items.Count && i < maxRows; i++)
            list.Items[i + scrollDepth].RenderItem(i, this, list.FocusIndex == i + scrollDepth);
        Renderer.HideCursor(MenuTextStyle);
    }
    private void RenderTitle(string title)
    {
        Point p = TableGrid.GetPoint("labels", "title");
        Renderer.PrintAtTile(p, MenuTextStyle, title);
    }

    public static string CenterAlign(string text, int length)
    {
        int offset = (length - text.Length) / 2;
        text += "".PadRight(offset);
        return text.PadLeft(length);
    }
}

public class MenuList
{
    public MenuList ParentMenu;
    public GameApp Game;

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

    public MenuList(string title, GameApp game)
    {
        Game = game;
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
            else Game.OpenMenu(ParentMenu);
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
}

public abstract class MenuItem
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

class MenuText : MenuItem
{
    internal string Subtitle;
    internal MenuText(string title, string sub = "") : base(title)
    {
        Subtitle = sub;
        Focusable = false;
    }

    internal override bool HandleItemActions(InputAction ia) => false;

    internal override void RenderItemExtras(int row, MenuVisualizer mv, bool focus)
    {
        StyleData s = focus ? mv.FocusBoxStyle : mv.MenuTextStyle;
        string text = MenuVisualizer.CenterAlign(Subtitle, mv.TableGrid.ColumnSeries("options").Width);
        mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("options", 0, "items", row), mv.MenuTextStyle,text);
    }
}

class MenuButton : MenuItem
{
    internal string Subtitle;
    internal MenuButton(string title, string sub = "") : base("[ " + title + " ]") => Subtitle = sub;

    internal override bool HandleItemActions(InputAction ia)
    {
        if (ia == InputAction.Dig) OnValueChanged();
        return (ia == InputAction.Dig);
    }

    internal override void RenderItemExtras(int row, MenuVisualizer mv, bool focus) { }
}

internal class MenuChoice<TOption> : MenuItem
{
    private readonly List<TOption> Options;
    private int SelectedIndex = 0;
    private readonly Func<TOption, string> Display;
    public bool Enabled = true;

    internal void SelectValue(TOption value)
    {
        if (value == null) throw new NullReferenceException("Item is null");
        if (!this.Select(value, true)) throw new Exception("Selected item not in the list");
    }

    internal MenuChoice(string title, List<TOption> options, Func<TOption, string> Display) : base(title)
    {
        Options = options;
        this.Display = Display;
        Title = title;
    }

    internal TOption SelectedOption => Options[SelectedIndex];
    internal string SelectedName => Display(SelectedOption);
    internal int Index { get => SelectedIndex; set => SetIndex(value); }

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
        }

        return false;
    }

    private void SetIndex(int value)
    {
        SelectedIndex = 0;
        if (Options.Count == 0) return;

        SelectedIndex = value;
        while (SelectedIndex >= Options.Count) SelectedIndex -= Options.Count;
        while (SelectedIndex < 0) SelectedIndex += Options.Count;
        OnValueChanged();
    }

    public bool Select(TOption option, bool silent = false)
    {
        int id = Options.IndexOf(option);
        if (id != -1)
        {
            if (silent) SelectedIndex = id;
            else Index = id;
        }
        return (id != -1);
    }

    override internal void RenderItemExtras(int row, MenuVisualizer mv, bool focus)
    {
        string text = MenuVisualizer.CenterAlign(SelectedName, mv.TableGrid.ColumnSeries("options").Width);
        StyleData styl = focus ? mv.FocusBoxStyle : mv.MenuTextStyle;

        if (Enabled)
        {
            mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("pre-options", 0, "items", row), mv.MenuTextStyle, mv.Game.Settings.Texts["menu-choice-left"]);
            mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("options", 0, "items", row), styl, text);
            mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("post-options", 0, "items", row), mv.MenuTextStyle, mv.Game.Settings.Texts["menu-choice-right"]);
        }
        else
        {
            mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("option", 0, "items", row), styl, text);
        }
    }
}

class MenuNumberRange : MenuChoice<int>
{
    public readonly int Min;
    public readonly int Max;

    public MenuNumberRange(string title, int min, int max) : base(title, Range(min, max), x => x.ToString())
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
            newnum = newnum % (int)Math.Pow(10, Math.Floor(Math.Log10(newnum)));
            if (!Select(newnum)) return Select(Max);
        }
        return true;
    }
}

class MenuBoolOption : MenuChoice<bool>
{
    public MenuBoolOption(string title) : base(title, new List<bool>() { false, true }, x => x ? "Yes" : "No") { }
}
