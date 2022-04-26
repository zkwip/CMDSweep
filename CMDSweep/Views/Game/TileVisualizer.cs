using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using System;
using CMDSweep.Views.Game.State;

namespace CMDSweep.Views.Game;

class TileVisualizer : ITypeVisualizer<Point>
{
    private Difficulty _difficulty;
    private GameSettings _settings;
    private IRenderer _renderer;

    private BoardState _boardState;
    private bool _dead;
    private readonly int _gridSize;
    private readonly int _tileWidth;
    StyleData _borderStyle;
    private readonly StyledText _clearVisual;

    public TileVisualizer(IRenderer renderer, GameSettings settings, BoardState initialState)
    {
        _difficulty = initialState.Difficulty;
        _settings = settings;
        _renderer = renderer;

        _boardState = initialState;
        _dead = false;
        _gridSize = settings.Dimensions["cell-grid-size"];
        _tileWidth = settings.Dimensions["cell-size-x"];
        _borderStyle = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");
        _clearVisual = new ("  ", _borderStyle);
    }

    public void UpdateBoardState(BoardState state)
    {
        _boardState = state;
    }

    public void Visualize(Point p)
    {
        StyledText visual;

        if (IsOnBoard(p))
            visual = CellVisual(p);
        else if (IsBorder(p))
            visual = BorderVisual(p);
        else
            visual = _clearVisual;

        _renderer.PrintAtTile(_boardState.View.MapToRender(p), visual);
    }

    private bool IsBorder(Point p) => _boardState.Bounds.Grow(1).Contains(p) && !_boardState.Bounds.Contains(p);

    private bool IsOnBoard(Point p) => _boardState.Bounds.Contains(p);

    public TileVisual GetTileStyle(Point cl)
    {
        if (_dead)
        {
            if (_boardState.CellIsMine(cl))
            {
                if (_boardState.CellIsFlagged(cl)) 
                    return TileVisual.DeadMineFlagged;

                if (_boardState.CellIsDiscovered(cl)) 
                    return TileVisual.DeadMineExploded;

                return TileVisual.DeadMine;
            }
            else
            {
                if (_boardState.CellIsFlagged(cl)) 
                    return TileVisual.DeadWrongFlag;

                if (_boardState.CellIsDiscovered(cl)) 
                    return TileVisual.DeadDiscovered;

                return TileVisual.DeadUndiscovered;
            }
        }
        
        if (_boardState.CellIsDiscovered(cl) && _boardState.CellIsMine(cl)) 
            return TileVisual.DiscoveredMine;

        if (_boardState.CellIsDiscovered(cl)) 
            return TileVisual.Discovered;

        if (_boardState.CellIsFlagged(cl)) 
            return TileVisual.Flagged;

        if (_boardState.CellIsQuestionMarked(cl)) 
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
            if (!_difficulty.OnlyShowAtCursor || tileVisual != TileVisual.Discovered || _boardState.CellMineNumber(cl) <= 0)
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

        if (p.Equals(new Point(_boardState.BoardWidth, -1)))
            return new(_settings.Texts["border-corner-tr"], _borderStyle);

        if (p.Equals(new Point(-1, _boardState.BoardHeight)))
            return new(_settings.Texts["border-corner-bl"], _borderStyle);

        if (p.Equals(new Point(_boardState.BoardWidth, _boardState.BoardHeight)))
            return new(_settings.Texts["border-corner-br"], _borderStyle);

        // Edges
        if (p.Y == -1 || p.Y == _boardState.BoardHeight)
            return new(_settings.Texts["border-horizontal"], _borderStyle);

        if (p.X == -1 || p.X == _boardState.BoardWidth)
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
        int num = _boardState.CellMineNumber(cl);

        if (_difficulty.SubtractFlags) 
            num = _boardState.CellSubtractedMineNumber(cl);

        if (num > 0 && (IsCursor(cl) || _difficulty.OnlyShowAtCursor))
        {
            text = num.ToString();
            fg = _settings.Colors[string.Format("cell-{0}-discovered", num % 10)];
            return text;
        }

        return _settings.Texts["cell-empty"];
    }

    private bool IsCursor(Point cl) => _boardState.Cursor == cl;

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