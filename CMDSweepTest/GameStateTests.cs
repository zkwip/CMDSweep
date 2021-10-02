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

            Assert.AreEqual(mines, gs.Mines);
            Assert.AreEqual(mines, gs.MinesLeft);
        }
    }
}
