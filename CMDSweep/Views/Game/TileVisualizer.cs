using CMDSweep.Geometry;
using CMDSweep.Data;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using System;
using CMDSweep.Views.Game.State;
using CMDSweep.Layout.Text;

namespace CMDSweep.Views.Game;

class TileVisualizer : ITypeVisualizer<Point, GameState>
{
    private readonly GameSettings _settings;
    private readonly IRenderer _renderer;

    private readonly int _gridSize;
    private readonly int _tileWidth;

    private readonly StyleData _borderStyle;
    private readonly StyledText _clearVisual;

    public TileVisualizer(IRenderer renderer, GameSettings settings)
    {
        _settings = settings;
        _renderer = renderer;

        _gridSize = settings.Dimensions["cell-grid-size"];
        _tileWidth = settings.Dimensions["cell-size-x"];
        _borderStyle = settings.GetStyle("border-fg", "cell-bg-out-of-bounds");
        _clearVisual = new ("  ", _borderStyle);
    }

    public void Visualize(Point p, GameState gameState)
    {
        bool dead = gameState.ProgressState.PlayerState == PlayerState.Dead;
        BoardState boardState = gameState.BoardState;
        Difficulty difficulty = boardState.Difficulty;

        StyledText visual = GetVisual(p, boardState, difficulty, dead);

        _renderer.PrintAtTile(boardState.View.MapToRender(p), visual);
    }

    private StyledText GetVisual(Point p, BoardState boardState, Difficulty difficulty, bool dead)
    {
        if (IsOnBoard(p, boardState))
            return CellVisual(p, boardState, difficulty, dead);

        if (IsBorder(p, boardState))
            return BorderVisual(p, boardState);

        return _clearVisual;
    }

    private bool IsBorder(Point p, BoardState boardState) => boardState.Bounds.Grow(1).Contains(p) && !boardState.Bounds.Contains(p);

    private bool IsOnBoard(Point p, BoardState boardState) => boardState.Bounds.Contains(p);

    public TileVisual GetTileStyle(Point p, BoardState boardState, bool dead)
    {
        if (dead)
        {
            if (boardState.CellIsMine(p))
            {
                if (boardState.CellIsFlagged(p)) 
                    return TileVisual.DeadMineFlagged;

                if (boardState.CellIsDiscovered(p)) 
                    return TileVisual.DeadMineExploded;

                return TileVisual.DeadMine;
            }
            else
            {
                if (boardState.CellIsFlagged(p)) 
                    return TileVisual.DeadWrongFlag;

                if (boardState.CellIsDiscovered(p)) 
                    return TileVisual.DeadDiscovered;

                return TileVisual.DeadUndiscovered;
            }
        }
        
        if (boardState.CellIsDiscovered(p) && boardState.CellIsMine(p)) 
            return TileVisual.DiscoveredMine;

        if (boardState.CellIsDiscovered(p)) 
            return TileVisual.Discovered;

        if (boardState.CellIsFlagged(p)) 
            return TileVisual.Flagged;

        if (boardState.CellIsQuestionMarked(p)) 
            return TileVisual.QuestionMarked;
        
        if (p.X % _gridSize == 0 || p.Y % _gridSize == 0) 
            return TileVisual.UndiscoveredGrid;

        return TileVisual.Undiscovered;
    }

    public StyledText CellVisual(Point p, BoardState boardState, Difficulty difficulty, bool dead)
    {
        TileVisual tileVisual = GetTileStyle(p, boardState, dead);

        ConsoleColor fg = GetTileForeground(tileVisual);
        ConsoleColor bg = GetTileBackground(tileVisual);

        string text = GetTileText(p, boardState, difficulty, tileVisual, ref fg);

        // Cursor
        if (!dead && IsCursor(p, boardState))
        {
            if (!difficulty.OnlyShowAtCursor || tileVisual != TileVisual.Discovered || boardState.CellMineNumber(p) <= 0)
                fg = _settings.Colors["cell-selected"];

            if (text == _settings.Texts["cell-undiscovered"] || text == _settings.Texts["cell-empty"])
                text = _settings.Texts["cursor"];
        }

        StyleData data = new(fg, bg, false);
        text = Padding.PadText(text, _tileWidth, HorzontalAlignment.Left);

        return new(text, data);
    }

    public StyledText BorderVisual(Point p, BoardState boardState)
    {
        // Corners
        if (p.Equals(new Point(-1, -1)))
            return new(_settings.Texts["border-corner-tl"], _borderStyle);

        if (p.Equals(new Point(boardState.BoardWidth, -1)))
            return new(_settings.Texts["border-corner-tr"], _borderStyle);

        if (p.Equals(new Point(-1, boardState.BoardHeight)))
            return new(_settings.Texts["border-corner-bl"], _borderStyle);

        if (p.Equals(new Point(boardState.BoardWidth, boardState.BoardHeight)))
            return new(_settings.Texts["border-corner-br"], _borderStyle);

        // Edges
        if (p.Y == -1 || p.Y == boardState.BoardHeight)
            return new(_settings.Texts["border-horizontal"], _borderStyle);

        if (p.X == -1 || p.X == boardState.BoardWidth)
            return new(_settings.Texts["border-vertical"], _borderStyle);

        throw new ArgumentOutOfRangeException(nameof(p), $"The point {p} is not part of the board");
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

    private string GetTileText(Point cl, BoardState boardState, Difficulty difficulty, TileVisual tileVisual, ref ConsoleColor fg)
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
                text = DiscoveredTileText(cl, boardState, difficulty, ref fg);
                break;
        }

        return text;
    }

    private string DiscoveredTileText(Point p, BoardState boardState, Difficulty difficulty, ref ConsoleColor fg)
    {
        string text;
        int num = boardState.CellMineNumber(p);

        if (difficulty.SubtractFlags) 
            num = boardState.CellSubtractedMineNumber(p);

        if (num > 0 && (IsCursor(p, boardState) || !difficulty.OnlyShowAtCursor))
        {
            text = num.ToString();
            fg = _settings.Colors[string.Format("cell-{0}-discovered", num % 10)];
            return text;
        }

        return _settings.Texts["cell-empty"];
    }

    private bool IsCursor(Point cl, BoardState boardState) => boardState.Cursor == cl;

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