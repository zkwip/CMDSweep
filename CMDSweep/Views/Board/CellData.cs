using System;

namespace CMDSweep.Views.Board;

struct CellData
{
    public CellData(bool m, bool d, FlagMarking f, bool q) { Mine = m; Discovered = d; Flagged = f; QuestionMarked = q; }
    public bool Mine;
    public bool Discovered;
    public FlagMarking Flagged;
    public bool QuestionMarked;


    public static bool operator ==(CellData c1, CellData c2)
    {
        return c1.Mine == c2.Mine &&
               c1.Discovered == c2.Discovered &&
               c1.Flagged == c2.Flagged;
    }

    public static bool operator !=(CellData c1, CellData c2)
    {
        return c1.Mine != c2.Mine ||
               c1.Discovered != c2.Discovered ||
               c1.Flagged != c2.Flagged;
    }

    public override bool Equals(object? obj)
    {
        return obj is CellData data &&
               Mine == data.Mine &&
               Discovered == data.Discovered &&
               Flagged == data.Flagged &&
               QuestionMarked == data.QuestionMarked;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mine, Discovered, Flagged, QuestionMarked);
    }
}
