using CMDSweep.Geometry;
using CMDSweep.Views.Game.State;
using Xunit;

namespace CMDSweepTests.LayoutTests
{
    public class BoardViewStateTests
    {

        public BoardViewStateTests() { }

        [Fact]
        public void ViewPortShouldCorrectlyMapToRenderMask()
        {
            Scale scale = new(2, 1);
            Offset offset = new(0, 3);
            int scrollSafezoneDistance = 3;
            Rectangle renderMask = new(0, 3, 50, 17);
            Rectangle board = new(0, 0, 10, 10);

            BoardViewState _sut = new(scale, offset, scrollSafezoneDistance, renderMask, board);

            Assert.Equal(renderMask.Dimensions.ScaleBack(scale), _sut.ViewPort.Dimensions);
            Assert.Equal(renderMask.TopLeft.ScaleBack(scale).Shift(offset.Reverse), _sut.ViewPort.TopLeft);

            Assert.Equal(renderMask.TopLeft, _sut.MapToRender(_sut.ViewPort.TopLeft));
            Assert.Equal(renderMask.BottomRight, _sut.MapToRender(_sut.ViewPort.BottomRight));
        }
    }
}