using System;

namespace CMDSweep
{
    public class TableGrid
    {
        private LinearPartitioner cp;
        private LinearPartitioner rp;

        private Rectangle bounds;
        public Rectangle Bounds { get => bounds; set { bounds = value; Sync(); } }

        public TableGrid()
        {
            cp = new();
            rp = new();
            bounds = Rectangle.Zero;
        }

        public void Sync()
        {
            cp.Range = bounds.HorizontalRange;
            rp.Range = bounds.VerticalRange;
        }

        public int Columns => cp.Count;
        public int Rows => rp.Count;

        public void AddColumn(int conspace, int varspace, string name = "", int repeat = 1) => cp.AddPart(name, conspace, varspace, repeat);
        public void AddRow(int conspace, int varspace, string name = "", int repeat = 1) => rp.AddPart(name, conspace, varspace, repeat);

        public Point GetPoint(int col, int row) => new Point(ColStart(col), RowStart(row));
        public Point GetPoint(string col, string row) => new Point(cp[col].Start, rp[row].Start);
        public Point GetPoint(string col, int co, string row, int ro) => new Point(cp[col, co].Start, rp[row, ro].Start);

        public Point this[int col, int row] => new Point(cp[col].Start, rp[row].Start);
        public Point this[string col, int row] => new Point(cp[col].Start, rp[row].Start);
        public Point this[int col, string row] => new Point(cp[col].Start, rp[row].Start);
        public Point this[string col, string row] => new Point(cp[col].Start, rp[row].Start);
        public Rectangle GetCell(int col, int row) => new Rectangle(cp[col].Range, rp[row].Range);

        public Rectangle Column(string name, int offset = 0) => Column(cp[name].Offset(offset));
        public Rectangle Column(int offset) => Column(cp[offset]);
        public Rectangle Row(string name, int offset = 0) => Row(rp[name].Offset(offset));
        public Rectangle Row(int offset) => Row(rp[offset]);

        public Rectangle ColumnSeries(string name) => new Rectangle(cp.All(name), bounds.VerticalRange);
        public Rectangle RowSeries(string name) => new Rectangle(bounds.HorizontalRange, rp.All(name));

        public Rectangle Column(Partition col) => new Rectangle(col.Range, bounds.VerticalRange);
        public Rectangle Row(Partition row) => new Rectangle(bounds.HorizontalRange, row.Range);

        public int RowStart(int row)
        {
            Sync();
            return rp.PartStart(row);
        }

        public int ColStart(int col)
        {
            Sync();
            return cp.PartStart(col);
        }

        public int RowEnd(int row)
        {
            Sync();
            return rp.PartEnd(row);
        }

        public int ColEnd(int col)
        {
            Sync();
            return cp.PartEnd(col);
        }

        public Offset CenterOn(Point c)
        {
            Offset o = bounds.CenterOn(c);
            Sync();
            return o;
        }

        public Offset Shift(Offset o)
        {
            o = Bounds.Shift(o);
            Sync();
            return o;
        }

        public void FitAround(int scale = 0)
        {
            bounds.Width = cp.ConstantSum + cp.VariableSum * scale;
            bounds.Height = rp.ConstantSum + rp.VariableSum * scale;
            Sync();
        }
    }
    class TableDimensionType
    {
        public int space = 0;
        public string name = "";

        public TableDimensionType(int s, string n) { space = s; name = n; }
        public override string ToString() => String.Format("\"{0}\" ({1})", name, space);
    }

}