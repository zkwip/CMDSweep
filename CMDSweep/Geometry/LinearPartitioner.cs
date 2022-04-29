using System;
using System.Collections.Generic;

namespace CMDSweep.Geometry;

class LinearPartitioner
{
    public LinearRange Range;

    public LinearPartitioner()
    {
        parts = new List<Partition>();
        Range = LinearRange.Zero;
    }

    internal List<Partition> parts;

    public Partition this[int index] => parts[index];

    public Partition this[string name] => FindFirst(name);

    public Partition this[string name, int index] => FindFirst(name).Offset(index);

    public Partition FindFirst(string name)
    {
        Partition? res = parts.Find(x => x.Name == name);
        if (res == null)
            throw new KeyNotFoundException($"Could not find a partition with the name \"{name}\".");

        return res;
    }

    public void Clear() => parts.Clear();

    public Partition FindLast(string name)
    {
        Partition? res = parts.FindLast(x => x.Name == name);
        if (res == null)
            throw new KeyNotFoundException($"Could not find a partition with the name \"{name}\".");

        return res;
    }

    public LinearRange PartRange(int index) => new(PartStart(index), PartEnd(index) - PartStart(index));

    public int PartStart(int index)
    {
        if (index > parts.Count) throw new IndexOutOfRangeException(String.Format("index out of range: {0} ", index));
        if (index < 0) throw new IndexOutOfRangeException(String.Format("index out of range: {0} ", index));

        int res = Range.Start + ConstTill(index);

        if (VariableSum != 0)
            res += (VarTill(index) * (Range.Length - ConstantSum) / VariableSum);

        return res;

    }
    public int PartEnd(int index) => PartStart(index + 1);

    private int ConstTill(int end) => Apply((s, p) => s + p.Constant, 0, end);
    private int VarTill(int end) => Apply((s, p) => s + p.Variable, 0, end);
    public int ConstantSum => ConstTill(parts.Count);
    public int VariableSum => VarTill(parts.Count);


    public int Count => parts.Count;

    private TOut Apply<TOut>(Func<TOut, Partition, TOut> f, TOut init, int end)
    {
        if (end > parts.Count) throw new IndexOutOfRangeException(String.Format("index out of range: {0} ", end));
        if (end == -1) end = parts.Count;
        for (int i = 0; i < end; i++) init = f(init, parts[i]);
        return init;
    }

    public void AddPart(string name, int con, int var = 0, int count = 1)
    {
        for (int i = 0; i < count; i++)
            parts.Add(new(con, var, name, this));
    }

    public LinearRange All(string name) => LinearRange.ToEnd(FindFirst(name).Start, FindLast(name).End);
}
