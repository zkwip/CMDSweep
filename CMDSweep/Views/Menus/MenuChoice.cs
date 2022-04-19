using CMDSweep.Rendering;
using System;
using System.Collections.Generic;

namespace CMDSweep.Views.Menus;

class MenuChoice<TOption> : MenuItem
{
    private readonly List<TOption> Options;
    private int SelectedIndex = 0;
    private readonly Func<TOption, string> Display;
    public bool Enabled = true;

    internal void SelectValue(TOption value)
    {
        if (value == null) throw new NullReferenceException("Item is null");
        if (!Select(value, true)) throw new Exception("Selected item not in the list");
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
        return id != -1;
    }

    override internal void RenderItemExtras(int row, MenuVisualizer mv, bool focus)
    {
        string text = MenuVisualizer.CenterAlign(SelectedName, mv.TableGrid.ColumnSeries("options").Width);
        StyleData styl = focus ? mv.FocusBoxStyle : mv.MenuTextStyle;

        if (Enabled)
        {
            mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("pre-options", 0, "items", row), mv.MenuTextStyle, mv.Settings.Texts["menu-choice-left"]);
            mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("options", 0, "items", row), styl, text);
            mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("post-options", 0, "items", row), mv.MenuTextStyle, mv.Settings.Texts["menu-choice-right"]);
        }
        else
        {
            mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("option", 0, "items", row), styl, text);
        }
    }
}
