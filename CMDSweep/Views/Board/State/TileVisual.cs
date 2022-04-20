namespace CMDSweep.Views.Board.State;

enum TileVisual
{
    Undiscovered,
    Flagged,
    QuestionMarked,
    Discovered,
    DiscoveredMine,

    DeadUndiscovered,
    DeadDiscovered,
    DeadMine,
    DeadMineExploded,
    DeadMineFlagged,
    DeadWrongFlag,
    UndiscoveredGrid,
}
