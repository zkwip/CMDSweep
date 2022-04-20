using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using System;
using CMDSweep.Views.Board.State;

namespace CMDSweep.Views.Board;

class TileVisualizer : ITypeVisualizer<Point>
{
    private Difficulty _difficulty;
    private GameSettings _settings;
    private IRenderer _renderer;

    private BoardView _view;
    private BoardData _boardData;
    private bool _dead;
    private readonly int _gridSize;
    private readonly int _tileWidth;
    StyleData _borderStyle;
    private readonly StyledText _clearVisual;

    public TileVisualizer(BoardState state, GameSettings settings, IRenderer renderer)
    {
        _difficulty = state.Difficulty;
        _settings = settings;
        _renderer = renderer;

        _view = state.View;
        _boardData = state.BoardData;
        _dead = state.RoundState.PlayerState == PlayerState.Dead;
        _gridSize = settings.Dimensions["cell-grid-size"];
        _tileWidth = settings.Dimensions["cell-size-x"];
        _borderStyle = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");
    }

    public void Visualize(Point p, RefreshMode _) => Visualize(p);
    public void Visualize(Point p)
    {
        StyledText visual;
        if (IsOnBoard(p))
            visual = CellVisual(p);
        else if (IsBorder(p))
            visual = CellVisual(p);
        else
            visual = _clearVisual;

        _renderer.PrintAtTile(_view.MapToRender(p), visual);
    }

    private bool IsBorder(Point p) => _boardData.Bounds.Grow(1).Contains(p) && !_boardData.Bounds.Contains(p);

    private bool IsOnBoard(Point p) => _boardData.Bounds.Contains(p);

    public TileVisual GetTileStyle(Point cl)
    {
        if (_dead)
        {
            if (_boardData.CellIsMine(cl))
            {
                if (_boardData.CellIsFlagged(cl)) 
                    return TileVisual.DeadMineFlagged;

                if (_boardData.CellIsDiscovered(cl)) 
                    return TileVisual.DeadMineExploded;

                return TileVisual.DeadMine;
            }
            else
            {
                if (_boardData.CellIsFlagged(cl)) 
                    return TileVisual.DeadWrongFlag;

                if (_boardData.CellIsDiscovered(cl)) 
                    return TileVisual.DeadDiscovered;

                return TileVisual.DeadUndiscovered;
            }
        }
        
        if (_boardData.CellIsDiscovered(cl) && _boardData.CellIsMine(cl)) 
            return TileVisual.DiscoveredMine;

        if (_boardData.CellIsDiscovered(cl)) 
            return TileVisual.Discovered;

        if (_boardData.CellIsFlagged(cl)) 
            return TileVisual.Flagged;

        if (_boardData.CellIsQuestionMarked(cl)) 
            return TileVisual.QuestionMarked;
        
        if (cl.X % _gridSize == 0 || cl.Y % _gridSize == 0) 
            return TileVisual.UndiscoveredGrid;

        return TileVisual.Undiscovered;
    }

    public StyledText CellVisual(Point cl)
    {
        TileVisual tileVisual = GetTileStyle(cl);

        ConsoleColor fg = GetTileForeground(tileVisual);
        ConsoleColor bg = GetTileBackground(tileVisual);
        string text = GetTileText(cl, tileVisual, ref fg);

        // Cursor
        if (!_dead && IsCursor(cl))
        {
            if (!_difficulty.OnlyShowAtCursor || tileVisual != TileVisual.Discovered || _boardData.CellMineNumber(cl) <= 0)
                fg = _settings.Colors["cell-selected"];

            if (text == _settings.Texts["cell-undiscovered"] || text == _settings.Texts["cell-empty"])
                text = _settings.Texts["cursor"];
        }

        StyleData data = new(fg, bg, false);
        text = Text.PadText(text, _tileWidth, HorzontalAlignment.Left);

        return new(text, data);
    }

    public StyledText BorderVisual(Point p)
    {

        // Corners
        if (p.Equals(new Point(-1, -1)))
            return new(_settings.Texts["border-corner-tl"], _borderStyle);

        if (p.Equals(new Point(_boardData.BoardWidth, -1)))
            return new(_settings.Texts["border-corner-tr"], _borderStyle);

        if (p.Equals(new Point(-1, _boardData.BoardHeight)))
            return new(_settings.Texts["border-corner-bl"], _borderStyle);

        if (p.Equals(new Point(_boardData.BoardWidth, _boardData.BoardHeight)))
            return new(_settings.Texts["border-corner-br"], _borderStyle);

        // Edges
        if (p.Y == -1 || p.Y == _boardData.BoardHeight)
            return new(_settings.Texts["border-horizontal"], _borderStyle);

        if (p.X == -1 || p.X == _boardData.BoardWidth)
            return new(_settings.Texts["border-vertical"], _borderStyle);

        throw new ArgumentOutOfRangeException();
    }


    private ConsoleColor GetTileBackground(TileVisual tileVisual)
    {
        return tileVisual switch
        {
            TileVisual.Discovered or
            TileVisual.DeadDiscovered or
            TileVisual.DeadMineExploded or
            TileVisual.DiscoveredMine => _settings.Colors["cell-bg-discovered"],

            TileVisual.UndiscoveredGrid => _settings.Colors["cell-bg-undiscovered-grid"],

            _ => _settings.Colors["cell-bg-undiscovered"]
        };
    }

    private string GetTileText(Point cl, TileVisual tileVisual, ref ConsoleColor fg)
    {
        string text = _settings.Texts["cell-empty"];

        switch (tileVisual)
        {
            case TileVisual.Undiscovered:
            case TileVisual.DeadUndiscovered:
            case TileVisual.UndiscoveredGrid:
                text = _settings.Texts["cell-undiscovered"];
                break;

            case TileVisual.DeadWrongFlag:
            case TileVisual.DeadMineFlagged:
            case TileVisual.Flagged:
                text = _settings.Texts["cell-flag"];
                break;

            case TileVisual.DeadMine:
            case TileVisual.DiscoveredMine:
            case TileVisual.DeadMineExploded:
                text = _settings.Texts["cell-mine"];
                break;

            case TileVisual.QuestionMarked:
                text = _settings.Texts["cell-questionmarked"];
                break;

            case TileVisual.DeadDiscovered:
            case TileVisual.Discovered:
                text = DiscoveredTileText(cl, ref fg);
                break;
        }

        return text;
    }

    private string DiscoveredTileText(Point cl, ref ConsoleColor fg)
    {
        string text;
        int num = _boardData.CellMineNumber(cl);

        if (_difficulty.SubtractFlags) 
            num = _boardData.CellSubtractedMineNumber(cl);

        if (num > 0 && (IsCursor(cl) || _difficulty.OnlyShowAtCursor))
        {
            text = num.ToString();
            fg = _settings.Colors[string.Format("cell-{0}-discovered", num % 10)];
            return text;
        }

        return _settings.Texts["cell-empty"];
    }

    private bool IsCursor(Point cl) => _boardData.Cursor == cl;

    private ConsoleColor GetTileForeground(TileVisual tileVisual)
    {
        return tileVisual switch
        {
            TileVisual.Discovered => _settings.Colors["cell-fg-discovered"],
            TileVisual.Undiscovered => _settings.Colors["cell-fg-undiscovered"],
            TileVisual.UndiscoveredGrid => _settings.Colors["cell-fg-undiscovered-grid"],
            TileVisual.Flagged => _settings.Colors["cell-flagged"],
            TileVisual.DiscoveredMine => _settings.Colors["cell-mine-discovered"],
            TileVisual.DeadWrongFlag => _settings.Colors["cell-dead-wrong-flag"],
            TileVisual.DeadMine => _settings.Colors["cell-dead-mine-missed"],
            TileVisual.DeadMineExploded => _settings.Colors["cell-dead-mine-hit"],
            TileVisual.DeadMineFlagged => _settings.Colors["cell-dead-mine-flagged"],
            TileVisual.DeadDiscovered => _settings.Colors["cell-fg-discovered"],
            TileVisual.DeadUndiscovered => _settings.Colors["cell-fg-undiscovered"],
            TileVisual.QuestionMarked => _settings.Colors["cell-questionmarked"],
            _ => _settings.Colors["cell-fg-out-of-bounds"],
        };
    }
}