using CMDSweep.Geometry;
using CMDSweep.Rendering;
using System;

namespace CMDSweep.Layout.Popup;

internal class TableGridPopup : IPlaceable
{
    private TableGrid _tableGrid;
    private Action<TableGrid, IRenderer> _renderTableContents;
    public StyleData TextStyle { get; private init; }

    public TableGridPopup(TableGrid tableGrid, StyleData textStyle, Action<TableGrid, IRenderer> renderTableContents)
    {
        _tableGrid = tableGrid;
        _renderTableContents = renderTableContents;
        TextStyle = textStyle;
    }

    public int Id => 0;

    public Dimensions ContentDimensions => _tableGrid.Bounds.Dimensions;

    public void RenderContent(Rectangle bounds, IRenderer renderer)
    {
        _tableGrid.Bounds = bounds;
        _renderTableContents(_tableGrid, renderer);
    }
}
