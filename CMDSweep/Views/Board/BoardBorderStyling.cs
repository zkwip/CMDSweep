using CMDSweep.Geometry;
using CMDSweep.IO;
using CMDSweep.Rendering;
using System;

namespace CMDSweep.Views.Board;

internal class BoardBorderStyling
{
    GameSettings _settings;
    BoardData _boardData;

    public BoardBorderStyling(GameSettings settings)
    {
        _settings = settings;
        _borderStyle = _settings.GetStyle("border-fg", "cell-bg-out-of-bounds");
    }

    StyleData _borderStyle;
    void RenderBorder()
    {
        // Top
        MappedPrint(-1, -1, _borderStyle, _settings.Texts["border-corner-tl"]);
        for (int x = 0; x < _boardData.BoardWidth; x++) MappedPrint(x, -1, _borderStyle, _settings.Texts["border-horizontal"]);
        MappedPrint(_boardData.BoardWidth, -1, _borderStyle, _settings.Texts["border-corner-tr"]);

        // Sides
        for (int y = 0; y < _boardData.BoardHeight; y++)
        {
            MappedPrint(-1, y, _borderStyle, _settings.Texts["border-vertical"]);
            MappedPrint(_boardData.BoardWidth, y, _borderStyle, _settings.Texts["border-vertical"]);
        }

        // Bottom
        MappedPrint(-1, _boardData.BoardHeight, _borderStyle, _settings.Texts["border-corner-bl"]);
        for (int x = 0; x < _boardData.BoardWidth; x++) MappedPrint(x, _boardData.BoardHeight, _borderStyle, _settings.Texts["border-horizontal"]);
        MappedPrint(_boardData.BoardWidth, _boardData.BoardHeight, _borderStyle, _settings.Texts["border-corner-br"]);
    }

    public StyledText GetBorderStyle(Point p)
    {
        StyleData data = _settings.GetStyle("border-fg", "cell-bg-out-of-bounds");

        // Corners
        if (p.Equals(new Point(-1, -1)))
            return new(_settings.Texts["border-corner-tl"], data);

        if (p.Equals(new Point(_boardData.BoardWidth, -1)))
            return new(_settings.Texts["border-corner-tr"], data);

        if (p.Equals(new Point(-1, _boardData.BoardHeight)))
            return new(_settings.Texts["border-corner-bl"], data);

        if (p.Equals(new Point(_boardData.BoardWidth, _boardData.BoardHeight)))
            return new(_settings.Texts["border-corner-br"], data);

        // Edges
        if (p.Y == -1 || p.Y == _boardData.BoardHeight)
            return new(_settings.Texts["border-horizontal"], data);

        if (p.X == -1 || p.X == _boardData.BoardWidth)
            return new(_settings.Texts["border-vertical"], data);

        throw new ArgumentOutOfRangeException();
    }

}
