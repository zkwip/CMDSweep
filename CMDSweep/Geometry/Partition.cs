namespace CMDSweep;

class Partition
{
    public int Constant;
    public int Variable;
    public string Name;
    public LinearPartitioner ap;
    public int Index => ap.parts.IndexOf(this);
    public LinearRange Range => ap.PartRange(Index);

    public int Start => Range.Start;
    public int End => Range.End;
    public int Length => Range.Length;
    public Partition(int c, int v, string n, LinearPartitioner a) { Constant = c; Variable = v; Name = n; ap = a; }
    public Partition Clone() => new(Constant, Variable, Name, ap);

    public Partition Offset(int offset) => ap[Index + offset];
}
