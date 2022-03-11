using System;
using System.Collections.Generic;

namespace CMDSweep;

internal class MenuVisualizer : Visualizer<MenuList>
{
    internal TableGrid TableGrid;
    internal int scrollDepth = 0;
    internal int maxRows = 4;
    public StyleData MenuTextStyle => Settings.GetStyle("menu");
    public StyleData FocusBoxStyle => Settings.GetStyle("menu-highlight-box");
    public StyleData FocusTitleStyle => Settings.GetStyle("menu-highlight-title");
    internal Dictionary<string, ConsoleColor> Colors => Settings.Colors;

    public MenuVisualizer(MenuController mctrl) : base(mctrl) 
    {
        HideStyle = Settings.GetStyle("menu");
        Resize();
    }

    internal override bool CheckResize() => false;
    internal override void Resize()
    {
        Dictionary<string, int> dims = Settings.Dimensions;
        TableGrid tg = new();

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
        
        TableGrid = tg;
    }

    internal override void RenderFull()
    {
        Renderer.ClearScreen(HideStyle);
        scrollDepth = CurrentState!.FixScroll(scrollDepth, maxRows);
        RenderTitle(CurrentState!.Title);
        for (int i = 0; i + scrollDepth < CurrentState!.Items.Count && i < maxRows; i++)
            CurrentState!.Items[i + scrollDepth].RenderItem(i, this, CurrentState!.FocusIndex == i + scrollDepth);
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

    internal override void RenderChanges() => RenderFull();

    internal override bool CheckScroll() => false;

    internal override void Scroll() { }

    internal override bool CheckFullRefresh() => false;

    internal override MenuList RetrieveState() => ((MenuController)Controller).currentMenuList;
}
