using CMDSweep.Data;
using CMDSweep.IO;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Menus.MenuItems;

class OptionMenuItem<TOption> : MenuItem
{
    private readonly List<TOption> Options;
    private int SelectedIndex = 0;
    private readonly Func<TOption, string> Display;

    private readonly string _symbolLeft;
    private readonly string _symbolRight;
    private readonly StyleData _focusBoxStyle;
    private readonly StyleData _menuTextStyle;

    public bool Enabled = true;

    internal OptionMenuItem(string title, List<TOption> options, Func<TOption, string> Display, GameSettings settings) : base(title)
    {
        Options = options;
        this.Display = Display;
        Title = title;

        _symbolLeft = settings.Texts["menu-choice-left"];
        _symbolRight = settings.Texts["menu-choice-right"];

        _menuTextStyle = settings.GetStyle("menu");
        _focusBoxStyle = settings.GetStyle("menu-highlight-box");
    }

    internal void SelectValue(TOption value)
    {
        if (value == null)
            throw new NullReferenceException("Item is null");

        if (!Select(value, true))
            throw new Exception("The selected item not in the list");
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
        return id != -1;
    }

    override internal void RenderItemExtras(IRenderer renderer, int row, TableGrid tableGrid, bool focus)
    {
        string text = CenterAlign(SelectedName, tableGrid.ColumnSeries("options").Width);

        StyleData styl = focus ? _focusBoxStyle : _menuTextStyle;

        if (Enabled)
        {
            renderer.PrintAtTile(tableGrid.GetPoint("pre-options", 0, "items", row), _menuTextStyle, _symbolLeft);
            renderer.PrintAtTile(tableGrid.GetPoint("options", 0, "items", row), styl, text);
            renderer.PrintAtTile(tableGrid.GetPoint("post-options", 0, "items", row), _menuTextStyle, _symbolRight);
        }
        else
        {
            renderer.PrintAtTile(tableGrid.GetPoint("option", 0, "items", row), styl, text);
        }
    }
}
