using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CMDSweep;

namespace CMDSweepTest
{
    [TestClass]
    public class GameStateTests
    {
        [TestMethod]
        public void NewGameTest()
        {
            int width = 10;
            int height = 20;
            int mines = 30;
            int safezone = 2;
            int radius = 1;

            GameState gs = GameState.NewGame(width, height, mines, safezone, radius);

            Assert.AreEqual(width, gs.BoardWidth);
            Assert.AreEqual(height, gs.BoardHeight);

            CellLocation loc = new CellLocation(5, 5);

            gs.SetCursor(loc);

            Assert.AreEqual(5, gs.Cursor.X);
            Assert.AreEqual(5, gs.Cursor.Y);

            gs.Dig();

            Assert.AreEqual(true, gs.CellIsDiscovered(5,5));
            Assert.AreEqual(false, gs.CellIsMine(5, 5));
            Assert.AreEqual(false, gs.CellIsFlagged(5, 5));

            Assert.AreEqual(mines, gs.CountMines);
            Assert.AreEqual(mines, gs.MinesLeft);
            Assert.AreEqual(0, gs.CellMineNumber(5, 5));

            GameState gs2 = gs.Clone();

            Assert.AreEqual(true, gs2.CellIsDiscovered(5, 5));
            Assert.AreEqual(false, gs2.CellIsMine(5, 5));
            Assert.AreEqual(false, gs2.CellIsFlagged(5, 5));

            Assert.AreEqual(mines, gs2.CountMines);
            Assert.AreEqual(mines, gs2.MinesLeft);
            Assert.AreEqual(0, gs2.CellMineNumber(5, 5));

        }
    }
}
