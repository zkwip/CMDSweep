using Xunit;
using CMDSweep.Geometry;
using CMDSweep.Views.Game.State;

namespace CMDSweepTests.LayoutTests
{
    public class BoardViewStateTests
    {
        BoardViewState _sut;

        public BoardViewStateTests()
        {

        }

        [Fact]
        public void ViewPortShouldCorrectlyMapToRenderMask()
        {
            Scale scale = new Scale(2, 1);
            Offset offset = new Offset(0, 3);
            int scrollSafezoneDistance = 3;
            Rectangle renderMask = new Rectangle(0, 3, 50, 17);
            Rectangle board = new Rectangle(0, 0, 10, 10);

            _sut = new BoardViewState(scale, offset, scrollSafezoneDistance, renderMask, board);

            Assert.Equal(renderMask.Dimensions.ScaleBack(scale), _sut.ViewPort.Dimensions);
            Assert.Equal(renderMask.TopLeft.ScaleBack(scale).Shift(offset.Reverse), _sut.ViewPort.TopLeft);

            Assert.Equal(renderMask.TopLeft, _sut.MapToRender(_sut.ViewPort.TopLeft));
            Assert.Equal(renderMask.BottomRight, _sut.MapToRender(_sut.ViewPort.BottomRight));
        }
    }
}