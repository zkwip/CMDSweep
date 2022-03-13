using CMDSweep;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SweepTests
{
    [TestClass]
    public class RectTests
    {
        [TestMethod]
        public void RectangleCreation()
        {
            Rectangle r = Rectangle.Zero;
            Assert.AreEqual(0, r.Left);
            Assert.AreEqual(0, r.Top);
            Assert.AreEqual(0, r.Right);
            Assert.AreEqual(0, r.Bottom);
            Assert.AreEqual(0, r.Width);
            Assert.AreEqual(0, r.Height);

            r = new Rectangle(new LinearRange(5, 10), new LinearRange(15, 5));
            Assert.AreEqual(5, r.Left);
            Assert.AreEqual(15, r.Top);
            Assert.AreEqual(15, r.Right);
            Assert.AreEqual(20, r.Bottom);
            Assert.AreEqual(10, r.Width);
            Assert.AreEqual(5, r.Height);

            Assert.AreEqual(r.HorizontalRange, new LinearRange(5, 10));
            Assert.AreEqual(r.VerticalRange, new LinearRange(15, 5));

            r = new Rectangle(new Point(1, 2), new Point(3, 4));
            Assert.AreEqual(1, r.Left);
            Assert.AreEqual(2, r.Top);
            Assert.AreEqual(3, r.Right);
            Assert.AreEqual(4, r.Bottom);
            Assert.AreEqual(2, r.Width);
            Assert.AreEqual(2, r.Height);

            r = new Rectangle(5, 6, 7, 8);
            Assert.AreEqual(5, r.Left);
            Assert.AreEqual(6, r.Top);
            Assert.AreEqual(12, r.Right);
            Assert.AreEqual(14, r.Bottom);
            Assert.AreEqual(7, r.Width);
            Assert.AreEqual(8, r.Height);

            Assert.AreEqual("((5, 6) to (12, 14), w: 7, h: 8)", r.ToString());

        }

        [TestMethod]
        public void RectangleDerivationTest()
        {
            Rectangle r = new(5, 6, 7, 8);

            Assert.AreEqual(r.Center.X, r.CenterLine);
            Assert.AreEqual(r.Center.Y, r.MidLine);

            Assert.IsFalse(r.Contains(new Point(1, 2)));
            Assert.IsFalse(r.Contains(new Point(8, 2)));
            Assert.IsTrue(r.Contains(new Point(8, 8)));
        }

        [TestMethod]
        public void RectangleCloneTest()
        {
            Rectangle r = new(5, 6, 7, 8);
            // Cloning
            Rectangle r2 = r.Clone();
            Assert.AreEqual(r, r2);

            Assert.AreEqual(r2.Center.X, r2.CenterLine);
            Assert.AreEqual(r2.Center.Y, r2.MidLine);

            Assert.IsFalse(r2.Contains(new Point(1, 2)));
            Assert.IsFalse(r2.Contains(new Point(8, 2)));
            Assert.IsTrue(r2.Contains(new Point(8, 8)));
        }

        [TestMethod]
        public void RectangleGrowTest()
        {
            Rectangle r = new(5, 6, 7, 8);

            // Growing
            Rectangle r3 = r.Grow(2);
            Assert.AreEqual(r.Center, r3.Center);
            Assert.AreEqual(new(3, 4), r3.TopLeft);
            Assert.AreEqual(new(14, 16), r3.BottomRight);

            r3 = r.Grow(1, 2, 3, 4);
            Assert.AreEqual(new(4, 4), r3.TopLeft);
            Assert.AreEqual(new(15, 18), r3.BottomRight);

            Rectangle r4 = r.Shrink(1, 2, 3, 4);
            Assert.AreEqual(new(6, 8), r4.TopLeft);
            Assert.AreEqual(new(9, 10), r4.BottomRight);

            Assert.IsTrue(r.Contains(r4.TopLeft));
            Assert.IsTrue(r.Contains(r4.BottomRight));
            Assert.IsTrue(r.Contains(r4));
            Assert.IsFalse(r.Contains(r3));
        }

        [TestMethod]
        public void RectangleShiftTest()
        {
            Rectangle r = new(5, 6, 7, 8);

            // Shifting
            Rectangle r5 = r.Shifted(10, 10);
            Rectangle r6 = r.Shifted(10, 10);

            Assert.AreEqual(r5,r6);
            Assert.IsFalse(r.Contains(r5));
            Assert.IsFalse(r5.Contains(r));

            r5.Shift(new Offset(-10, -10));
            r6.Shift(-10, -10);

            Assert.AreEqual(r, r5);
            Assert.AreEqual(r6, r5);

            r5.CenterOn(new(0, 0));
            Assert.AreEqual(Point.Origin, r5.Center);

        }

        [TestMethod]
        public void RectangleIntersectTest()
        {
            Rectangle r = new(0, 5, 10, 10);
            Rectangle r2 = new(5, 0, 10, 10);
            Rectangle r3 = r.Intersect(r2);

            Assert.AreEqual(new Point(5, 5), r3.TopLeft);
            Assert.AreEqual(new Point(10, 10), r3.BottomRight);

            Assert.IsTrue(r.Contains(r3));
            Assert.IsTrue(r2.Contains(r3));

            Assert.AreEqual(r.OffsetOutOfBounds(new(5, 0)), new(0, -5));
            Assert.AreEqual(r2.OffsetOutOfBounds(new(0, 5)), new(-5, 0));
        }
    }
}