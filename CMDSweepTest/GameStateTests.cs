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
            // Random settings
            Random r = new Random();
            int width = r.Next(5,30);
            int height = r.Next(5, 30);
            int mines = r.Next(5, 30);
            int safezone = r.Next(1,width / 2);
            int detectionRadius = r.Next(1,safezone);

            bool flags = (r.Next()%2 == 0);
            bool question = (r.Next() % 2 == 0);
            bool wrap = (r.Next() % 2 == 0);
            bool sub = (r.Next() % 2 == 0);
            bool onlycursor = (r.Next() % 2 == 0);
            bool automatic = (r.Next() % 2 == 0);

            Difficulty dif = new Difficulty()
            {
                Width = width,
                Height = height,
                Mines = mines,
                Safezone = safezone,
                DetectionRadius = detectionRadius,

                FlagsAllowed = flags,
                QuestionMarkAllowed = question,
                WrapAround = wrap,
                SubtractFlags = sub,
                OnlyShowAtCursor = onlycursor,
                AutomaticDiscovery = automatic,
            };

            GameBoardState gs = GameBoardState.NewGame(dif);

            Assert.AreEqual(width, gs.BoardWidth);
            Assert.AreEqual(height, gs.BoardHeight);

            CellLocation loc = new CellLocation(5, 5);

            gs.SetCursor(loc);

            Assert.AreEqual(5, gs.Cursor.X);
            Assert.AreEqual(5, gs.Cursor.Y);

            gs.Dig();

            CellLocation curs = new CellLocation(5, 5);

            Assert.AreEqual(true, gs.CellIsDiscovered(curs));
            Assert.AreEqual(false, gs.CellIsMine(curs));
            Assert.AreEqual(false, gs.CellIsFlagged(curs));

            Assert.AreEqual(mines, gs.Mines);
            Assert.AreEqual(mines, gs.MinesLeft);
            Assert.AreEqual(0, gs.CellMineNumber(curs));

            GameBoardState gs2 = gs.Clone();

            Assert.AreEqual(true, gs2.CellIsDiscovered(curs));
            Assert.AreEqual(false, gs2.CellIsMine(curs));
            Assert.AreEqual(false, gs2.CellIsFlagged(curs));

            Assert.AreEqual(mines, gs2.Mines);
            Assert.AreEqual(mines, gs2.MinesLeft);
            Assert.AreEqual(0, gs2.CellMineNumber(curs));

        }
    }
}
