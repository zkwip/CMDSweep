using CMDSweep.Geometry;
using CMDSweep.IO;
using CMDSweep.Layout;
using CMDSweep.Rendering;
using System;

namespace CMDSweep.Views.Board;

class TileStyling
{
    BoardData _board;
    Difficulty _difficulty;
    GameSettings _settings;

    bool _dead;
    readonly int _gridsize;
    readonly int _tileWidth;

    public TileVisual GetTileStyle(Point cl)
    {
        if (_dead)
        {
            if (_board.CellIsMine(cl))
            {
                if (_board.CellIsFlagged(cl)) 
                    return TileVisual.DeadMineFlagged;

                if (_board.CellIsDiscovered(cl)) 
                    return TileVisual.DeadMineExploded;

                return TileVisual.DeadMine;
            }
            else
            {
                if (_board.CellIsFlagged(cl)) 
                    return TileVisual.DeadWrongFlag;

                if (_board.CellIsDiscovered(cl)) 
                    return TileVisual.DeadDiscovered;

                return TileVisual.DeadUndiscovered;
            }
        }
        
        if (_board.CellIsDiscovered(cl) && _board.CellIsMine(cl)) 
            return TileVisual.DiscoveredMine;

        if (_board.CellIsDiscovered(cl)) 
            return TileVisual.Discovered;

        if (_board.CellIsFlagged(cl)) 
            return TileVisual.Flagged;

        if (_board.CellIsQuestionMarked(cl)) 
            return TileVisual.QuestionMarked;
        
        if (cl.X % _gridsize == 0 || cl.Y % _gridsize == 0) 
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
            if (!_difficulty.OnlyShowAtCursor || tileVisual != TileVisual.Discovered || _board.CellMineNumber(cl) <= 0)
                fg = _settings.Colors["cell-selected"];

            if (text == _settings.Texts["cell-undiscovered"] || text == _settings.Texts["cell-empty"])
                text = _settings.Texts["cursor"];
        }

        StyleData data = new(fg, bg, false);
        text = Text.PadText(text, _tileWidth, HorzontalAlignment.Left);

        return new(text, data);
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
        int num = _board.CellMineNumber(cl);

        if (_difficulty.SubtractFlags) 
            num = _board.CellSubtractedMineNumber(cl);

        if (num > 0 && (IsCursor(cl) || _difficulty.OnlyShowAtCursor))
        {
            text = num.ToString();
            fg = _settings.Colors[string.Format("cell-{0}-discovered", num % 10)];
            return text;
        }

        return _settings.Texts["cell-empty"];
    }

    private bool IsCursor(Point cl) => _board.Cursor == cl;

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