﻿using CMDSweep.Data;
using CMDSweep.Layout;
using CMDSweep.Rendering;

namespace CMDSweep.Views.Menus;

class MenuText : MenuItem
{
    internal string Subtitle;
    private StyleData _menuTextStyle;

    internal MenuText(GameSettings settings, string title, string sub = "") : base(title)
    {
        Subtitle = sub;
        Focusable = false;
        _menuTextStyle = settings.GetStyle("menu");
    }

    internal override bool HandleItemActions(InputAction ia) => false;

    internal override void RenderItemExtras(IRenderer renderer, int row, TableGrid tableGrid, bool focus)
    {
        string text = CenterAlign(Subtitle, tableGrid.ColumnSeries("options").Width);
        renderer.PrintAtTile(tableGrid.GetPoint("options", 0, "items", row), _menuTextStyle, text);
    }

}
