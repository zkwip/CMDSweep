using CMDSweep;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SweepTests
{
    [TestClass]
    public class TableGridTest
    {
        [TestMethod]
        public void TestAddition()
        {
            // Empty table
            TableGrid tg = new TableGrid();
            Assert.AreEqual(tg.Columns, 0);
            Assert.AreEqual(tg.Rows, 0);
            Assert.AreEqual(tg.Bounds, Rectangle.Zero);
            Assert.AreEqual(tg.GetPoint(0,0), new Point(0,0));

            // Add one row
            tg.AddRow(10, 0, "r1");
            Assert.AreEqual(tg.Columns, 0);
            Assert.AreEqual(tg.Rows, 1);
            Assert.AreEqual(tg.RowStart(0), 0);
            Assert.AreEqual(tg.RowEnd(0), 10);

            // Add one column
            tg.AddColumn(15, 0, "c1");
            Assert.AreEqual(tg.Columns, 1);
            Assert.AreEqual(tg.Rows, 1);
            Assert.AreEqual(tg.ColStart(0), 0);
            Assert.AreEqual(tg.ColEnd(0), 15);

            Assert.AreEqual(tg.GetPoint(0, 0), new Point(0, 0));
            Assert.AreEqual(tg.GetPoint(1, 1), new Point(15, 10));

            Assert.AreEqual(tg.GetPoint("c1", "r1"), tg.Bounds.TopLeft);
            Assert.AreEqual(tg.GetPoint("c1", 0, "r1", 0), tg.Bounds.TopLeft);

            // Move the table 
            tg.Bounds = tg.Bounds.Shifted(10, 5);
            Assert.AreEqual(new Point(10, 5), tg.Bounds.TopLeft);

            Assert.AreEqual(tg.Bounds.TopLeft, tg.GetPoint("c1", "r1"));
            Assert.AreEqual(tg.Bounds.TopLeft, tg.GetPoint("c1", 0, "r1", 0));

            // Add multiple columns
            tg.AddColumn(25, 0, "c2", 4);
            Assert.AreEqual(5, tg.Columns);
            Assert.AreEqual(1, tg.Rows);

            Assert.AreEqual(tg.GetPoint("c1", "r1"), tg.Bounds.TopLeft);
            Assert.AreEqual(tg.GetPoint("c1", 0, "r1", 0), tg.Bounds.TopLeft);

            Assert.AreEqual(tg.GetPoint("c2", "r1").X, 10 + 15);
            Assert.AreEqual(tg.GetPoint("c2", 0, "r1", 0).X, 10 + 15);

            // Add multiple rows
            tg.AddRow(1, 0, "r2", 5);
        }

        [TestMethod]
        public void TestRectOutputs()
        {
            TableGrid tg = new TableGrid();
            tg.Bounds = new Rectangle(10, 20, 90, 80);

            // Basic oddly spaced table
            tg.AddColumn(15, 0, "col", 2);
            tg.AddColumn(0, 1, "col", 2);
            tg.AddRow(5, 0, "row", 3);
            tg.AddRow(0, 1, "row", 3);

            // Check matching corners
            Assert.AreEqual(tg.GetCell(0, 0).TopLeft, tg.Bounds.TopLeft);
            Assert.AreEqual(tg.GetCell(0, 0).BottomRight, tg.GetCell(1, 1).TopLeft);

            // Check column selectors
            Assert.AreEqual(tg.Column("col"), tg.Column(0));
            Assert.AreEqual(tg.ColumnSeries("col"), tg.Bounds);

            // Check row selectors
            Assert.AreEqual(tg.Row("row"), tg.Row(0));
            Assert.AreEqual(tg.RowSeries("row"), tg.Bounds);

            // Move the table
            Offset shift = tg.CenterOn(Point.Origin);
            Assert.AreEqual(tg.Bounds.Center, Point.Origin);

            
            Assert.AreEqual(tg.GetCell(0, 0).TopLeft, tg.Bounds.TopLeft);
            Assert.AreEqual(tg.GetCell(0, 0).BottomRight, tg.GetCell(1, 1).TopLeft);

            Assert.AreEqual(tg.Column("col"), tg.Column(0));
            Assert.AreEqual(tg.ColumnSeries("col"), tg.Bounds);

            Assert.AreEqual(tg.Row("row"), tg.Row(0));
            Assert.AreEqual(tg.RowSeries("row"), tg.Bounds);
        }
    }
}