using CMDSweep.Geometry;

namespace CMDSweep.Layout;
class TableGrid : IBounded
{
    private readonly LinearPartitioner colPart;
    private readonly LinearPartitioner rowPart;
    private Rectangle bounds;

    public TableGrid()
    {
        bounds = Rectangle.Zero;
        colPart = new();
        rowPart = new();
    }

    private void setBounds(Rectangle b)
    {
        bounds = b;
        colPart.Range = b.HorizontalRange;
        rowPart.Range = b.VerticalRange;
    }

    public Rectangle Bounds { get => bounds; set => setBounds(value); }

    public int Columns => colPart.Count;

    public int Rows => rowPart.Count;

    public void AddColumn(int conspace, int varspace, string name = "", int repeat = 1) => colPart.AddPart(name, conspace, varspace, repeat);
    
    public void AddRow(int conspace, int varspace, string name = "", int repeat = 1) => rowPart.AddPart(name, conspace, varspace, repeat);

    public Point GetPoint(int col, int row) => new(ColStart(col), RowStart(row));
    
    public Point GetPoint(string col, string row) => new(colPart[col].Start, rowPart[row].Start);
    
    public Point GetPoint(string col, int co, string row, int ro) => new(colPart[col, co].Start, rowPart[row, ro].Start);
    
    public Rectangle GetCell(int col, int row) => new(colPart[col].Range, rowPart[row].Range);

    public Rectangle Column(string name, int offset = 0) => Column(colPart[name].Offset(offset));
    
    public Rectangle Column(int offset) => Column(colPart[offset]);
    
    public Rectangle Row(string name, int offset = 0) => Row(rowPart[name].Offset(offset));
    
    public Rectangle Row(int offset) => Row(rowPart[offset]);

    public Rectangle ColumnSeries(string name) => new(colPart.All(name), Bounds.VerticalRange);
    
    public Rectangle RowSeries(string name) => new(Bounds.HorizontalRange, rowPart.All(name));

    public Rectangle Column(Partition col) => new(col.Range, Bounds.VerticalRange);
    
    public Rectangle Row(Partition row) => new(Bounds.HorizontalRange, row.Range);

    public int RowStart(int row) => rowPart.PartStart(row);

    public int ColStart(int col) => colPart.PartStart(col);

    public int RowEnd(int row) => rowPart.PartEnd(row);

    public int ColEnd(int col) => colPart.PartEnd(col);
    
    public void CenterOn(Point p) => Bounds = Bounds.CenterOn(p);
    
    public void Shift(Offset o) => Bounds = Bounds.Shift(o);

    public Dimensions ContentFitDimensions(int variableScale = 0)
    {
        int width = colPart.ConstantSum + colPart.VariableSum * variableScale;
        int height = rowPart.ConstantSum + rowPart.VariableSum * variableScale;

        return new(width, height);
    }
}
