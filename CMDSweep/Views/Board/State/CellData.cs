namespace CMDSweep.Views.Board.State;

record struct CellData
{
    public CellData(bool m, bool d, FlagMarking f, bool q)
    {
        Mine = m;
        Discovered = d;
        Flagged = f;
        QuestionMarked = q;
    }

    public bool Mine;

    public bool Discovered;

    public FlagMarking Flagged;

    public bool QuestionMarked;
}
