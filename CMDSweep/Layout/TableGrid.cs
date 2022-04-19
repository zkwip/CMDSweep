using CMDSweep.Geometry;
using System;

namespace CMDSweep.Layout;
class TableGrid
{
    private readonly LinearPartitioner cp;
    private readonly LinearPartitioner rp;

    public Rectangle Bounds;

    public TableGrid()
    {
        cp = new();
        rp = new();
        Bounds = Rectangle.Zero;
    }

    public int Columns => cp.Count;
    public int Rows => rp.Count;

    public void AddColumn(int conspace, int varspace, string name = "", int repeat = 1) => cp.AddPart(name, conspace, varspace, repeat);
    public void AddRow(int conspace, int varspace, string name = "", int repeat = 1) => rp.AddPart(name, conspace, varspace, repeat);

    public Point GetPoint(int col, int row) => new(ColStart(col), RowStart(row));
    public Point GetPoint(string col, string row) => new(cp[col].Start, rp[row].Start);
    public Point GetPoint(string col, int co, string row, int ro) => new(cp[col, co].Start, rp[row, ro].Start);

    public Point this[int col, int row] => new(cp[col].Start, rp[row].Start);
    public Point this[string col, int row] => new(cp[col].Start, rp[row].Start);
    public Point this[int col, string row] => new(cp[col].Start, rp[row].Start);
    public Point this[string col, string row] => new(cp[col].Start, rp[row].Start);
    public Rectangle GetCell(int col, int row) => new(cp[col].Range, rp[row].Range);

    public Rectangle Column(string name, int offset = 0) => Column(cp[name].Offset(offset));
    public Rectangle Column(int offset) => Column(cp[offset]);
    public Rectangle Row(string name, int offset = 0) => Row(rp[name].Offset(offset));
    public Rectangle Row(int offset) => Row(rp[offset]);

    public Rectangle ColumnSeries(string name) => new(cp.All(name), Bounds.VerticalRange);
    public Rectangle RowSeries(string name) => new(Bounds.HorizontalRange, rp.All(name));

    public Rectangle Column(Partition col) => new(col.Range, Bounds.VerticalRange);
    public Rectangle Row(Partition row) => new(Bounds.HorizontalRange, row.Range);

    public int RowStart(int row) => rp.PartStart(row);

    public int ColStart(int col) => cp.PartStart(col);

    public int RowEnd(int row) => rp.PartEnd(row);

    public int ColEnd(int col) => cp.PartEnd(col);

    public void CenterOn(Point c) => Bounds = Bounds.CenterOn(c);

    public void Shift(Offset o) => Bounds = Bounds.Shift(o);

    public void FitAround(int scale = 0)
    {
        int width = cp.ConstantSum + cp.VariableSum * scale;
        int height = rp.ConstantSum + rp.VariableSum * scale;

        Bounds = new(Bounds.Left, Bounds.Top, width, height);
    }
}
/*
class TableDimensionType
{
    public int space = 0;
    public string name = "";

    public TableDimensionType(int s, string n) { space = s; name = n; }
    public override string ToString() => String.Format("\"{0}\" ({1})", name, space);
}
*/
