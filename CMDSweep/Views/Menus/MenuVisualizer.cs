using CMDSweep.Data;
using CMDSweep.Geometry;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using CMDSweep.Views.Menus.MenuItems;
using System.Collections.Generic;

namespace CMDSweep.Views.Menus;

class MenuVisualizer : ITypeVisualizer<MenuList>
{
    private readonly TableGrid _tableGrid;
    private readonly IRenderer _renderer;
    private readonly GameSettings _settings;

    private int scrollDepth = 0;
    private int maxRows = 4;

    private readonly StyleData _menuTextStyle;
    private readonly StyleData _focusTitleStyle;
    private readonly StyleData _hideStyle;

    public MenuVisualizer(GameSettings settings, IRenderer renderer)
    {
        _settings = settings;

        _hideStyle = settings.GetStyle("menu");
        _menuTextStyle = settings.GetStyle("menu");
        _focusTitleStyle = settings.GetStyle("menu-highlight-title");
        _renderer = renderer;
        _tableGrid = new();

        Resize();
    }

    public void Resize()
    {
        Dictionary<string, int> dims = _settings.Dimensions;
        maxRows = dims["menu-rows"];

        _tableGrid.Clear();

        // Columns
        _tableGrid.AddColumn(dims["menu-indent"], 0, "prefix");
        _tableGrid.AddColumn(dims["menu-col1-width"], 0, "labels");
        _tableGrid.AddColumn(dims["menu-box-padding"], 0);
        _tableGrid.AddColumn(dims["menu-box-padding"], 0, "pre-options");
        _tableGrid.AddColumn(dims["menu-col2-width"], 0, "options");
        _tableGrid.AddColumn(dims["menu-box-padding"], 0, "post-options");

        // Rows
        _tableGrid.AddRow(dims["menu-title-space"], 0, "title");
        _tableGrid.AddRow(dims["menu-row-scale"], 0, "items", maxRows);

        Dimensions dimensions = _tableGrid.ContentFitDimensions();
        _tableGrid.Bounds = Rectangle.Centered(_renderer.Bounds.Center, dimensions);
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
