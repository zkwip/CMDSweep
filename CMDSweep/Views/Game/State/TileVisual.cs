namespace CMDSweep.Views.Game.State;

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
