using CMDSweep;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SweepTests
{
    [TestClass]
    public class LinearPartitionerTests
    {
        [TestMethod]
        public void TestCreation()
        {
            LinearPartitioner ap = new LinearPartitioner();
            Assert.AreEqual(ap.Range, LinearRange.Zero);

            ap.Range = new(0, 100);

            Assert.AreEqual(ap.Range.End, 100);
        }

        [TestMethod]
        public void TestAddition()
        {
            // New Additions
            LinearPartitioner ap = new LinearPartitioner();
            ap.Range = new(0, 100);
            ap.AddPart("item 1", 0, 1);

            Assert.AreEqual(ap.Count, 1);
            Assert.AreEqual(ap["item 1"], ap[0]);
            Assert.AreEqual(ap[0].Range, new(0, 100));

            Assert.ThrowsException<KeyNotFoundException>(() => ap["item 2"]);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ap[1]);

            // Devide the room
            ap.AddPart("item 2", 0, 1);

            Assert.AreEqual(new(0, 50), ap[0].Range);
            Assert.AreEqual(new(50, 50), ap[1].Range);

            // Devide with another
            ap.AddPart("item 3", 50, 0);

            Assert.AreEqual(new(0, 25), ap[0].Range);
            Assert.AreEqual(new(25, 25), ap[1].Range);
            Assert.AreEqual(new(50, 50), ap[2].Range);

            // Reset the scale
            ap.Range.Length = 150;

            Assert.AreEqual(new(0, 50), ap[0].Range);
            Assert.AreEqual(new(50, 50), ap[1].Range);
            Assert.AreEqual(new(100, 50), ap[2].Range);


        }
    }
}