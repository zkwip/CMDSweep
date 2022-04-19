namespace CMDSweep.Views.Menus;

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
        string text = MenuVisualizer.CenterAlign(Subtitle, mv.TableGrid.ColumnSeries("options").Width);
        mv.Renderer.PrintAtTile(mv.TableGrid.GetPoint("options", 0, "items", row), mv.MenuTextStyle, text);
    }
}
