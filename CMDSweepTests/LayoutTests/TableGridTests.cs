using CMDSweep.Geometry;
using CMDSweep.Layout;
using System;
using Xunit;

namespace CMDSweepTests.LayoutTests
{
    public class TableGridTests
    {
        TableGrid _sut;

        public TableGridTests()
        {
            _sut = new TableGrid();
        }

        [Fact]
        public void SingleCellPartitionShouldMatchTheMainBounds()
        {
            Rectangle rect = new Rectangle(0, 0, 10, 10);
            _sut.Bounds = rect;
            _sut.AddColumn(0, 1);
            _sut.AddRow(0, 1);

            Assert.Equal(rect, _sut.GetCell(0, 0));
        }

        [Fact]
        public void EmptyGridShouldBeEmpty()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _sut.GetCell(0, 0);
            });

            Assert.Equal(0, _sut.Columns);
            Assert.Equal(0, _sut.Rows);
            Assert.Equal(Rectangle.Zero, _sut.Bounds);
            Assert.Equal(new Point(0, 0), _sut.GetPoint(0, 0));
        }

        [Fact]
        public void GridWithOneRowShouldHaveOneRow()
        {
            _sut.AddRow(10, 0, "r1");

            Assert.Equal(0, _sut.Columns);
            Assert.Equal(1, _sut.Rows);
            Assert.Equal(0, _sut.RowStart(0));
            Assert.Equal(10, _sut.RowEnd(0));
        }

        [Fact]
        public void GridWithOneColumnAndRowShouldHaveOneRow()
        {
            _sut.AddRow(10, 0, "r1");
            _sut.AddColumn(15, 0, "c1");

            Assert.Equal(1, _sut.Columns);
            Assert.Equal(1, _sut.Rows);
            Assert.Equal(0, _sut.ColStart(0));
            Assert.Equal(15, _sut.ColEnd(0));

            Assert.Equal(Point.Origin, _sut.GetPoint(0, 0));
            Assert.Equal(new Point(15, 10), _sut.GetPoint(1, 1));

            Assert.Equal(_sut.Bounds.TopLeft, _sut.GetPoint("c1", "r1"));
            Assert.Equal(_sut.Bounds.TopLeft, _sut.GetPoint("c1", 0, "r1", 0));
        }

        [Fact]
        public void GridWithMultipleColumns()
        {
            _sut.AddRow(10, 0, "r1");
            _sut.AddColumn(15, 0, "c1");
            _sut.Bounds = _sut.Bounds.Shift(10, 5);
            _sut.AddColumn(25, 0, "c2", 4);

            Assert.Equal(5, _sut.Columns);
            Assert.Equal(1, _sut.Rows);

            Assert.Equal(_sut.Bounds.TopLeft, _sut.GetPoint("c1", "r1"));
            Assert.Equal(_sut.Bounds.TopLeft, _sut.GetPoint("c1", 0, "r1", 0));

            Assert.Equal(10 + 15, _sut.GetPoint("c2", "r1").X);
            Assert.Equal(10 + 15, _sut.GetPoint("c2", 0, "r1", 0).X);

        }

        [Fact]
        public void PopulatedGridShouldNotAllowIndexingOutOfBounds()
        {
            Rectangle rect = new Rectangle(0, 0, 10, 10);
            _sut.Bounds = rect;
            _sut.AddColumn(0, 1);
            _sut.AddRow(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _sut.GetCell(1, 0);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _sut.GetCell(0, 1);
            });
        }
    }
}