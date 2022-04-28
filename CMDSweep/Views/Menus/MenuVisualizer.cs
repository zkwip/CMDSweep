using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using CMDSweep.Views.Menus.MenuItems;
using System.Collections.Generic;

namespace CMDSweep.Views.Menus;

class MenuVisualizer : ITypeVisualizer<MenuList>
{
    private TableGrid _tableGrid;
    private IRenderer _renderer;
    private GameSettings _settings;

    private int scrollDepth = 0;
    private int maxRows = 4;

    private StyleData _menuTextStyle;
    private StyleData _focusBoxStyle;
    private StyleData _focusTitleStyle;
    private StyleData _hideStyle;

    public MenuVisualizer(GameSettings settings, IRenderer renderer)
    {
        _settings = settings;

        _hideStyle = settings.GetStyle("menu");
        _menuTextStyle = settings.GetStyle("menu");
        _focusBoxStyle = settings.GetStyle("menu-highlight-box");
        _focusTitleStyle = settings.GetStyle("menu-highlight-title");
        _renderer = renderer;

        Resize();
    }

    public void Resize()
    {
        Dictionary<string, int> dims = _settings.Dimensions;
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

        Dimensions dimensions = tg.ContentFitDimensions();
        tg.Bounds = Rectangle.Centered(_renderer.Bounds.Center, dimensions);
        _tableGrid = tg;
    }

    public void Visualize(MenuList state)
    {
        _renderer.ClearScreen(_hideStyle);
        scrollDepth = state.FixScroll(scrollDepth, maxRows);

        RenderMenuTitle(state.Title);

        for (int i = 0; i + scrollDepth < state.Items.Count && i < maxRows; i++)
        {
            MenuItem item = state.Items[i + scrollDepth];
            RenderMenuItem(i, state.FocusIndex == i + scrollDepth, item);
        }

        //_renderer.HideCursor(_menuTextStyle);
    }

    private void RenderMenuTitle(string title)
    {
        Point p = _tableGrid.GetPoint("labels", "title");
        _renderer.PrintAtTile(p, _menuTextStyle, title);
    }

    internal void RenderMenuItem(int row, bool focus, MenuItem item)
    {
        StyleData styl = focus ? _focusTitleStyle : _menuTextStyle;

        _renderer.ClearScreen(_menuTextStyle, _tableGrid.Row("items", row));

        _renderer.PrintAtTile(_tableGrid.GetPoint("labels", 0, "items", row), styl, item.Title);

        item.RenderItemExtras(_renderer, row, _tableGrid, focus);
    }
}
