using CMDSweep.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SweepTests;

[TestClass]
public class LinearPartitionerTests
{
    [TestMethod]
    public void TestCreation()
    {
        LinearPartitioner lp = new();
        Assert.AreEqual(lp.Range, LinearRange.Zero);

        lp.Range = new(0, 100);

        Assert.AreEqual(lp.Range.End, 100);
    }

    [TestMethod]
    public void TestAddition()
    {
        // New Additions
        LinearPartitioner lp = new();
        lp.Range = new(0, 100);
        lp.AddPart("item 1", 0, 1);

        Assert.AreEqual(lp.Count, 1);
        Assert.AreEqual(lp["item 1"], lp[0]);
        Assert.AreEqual(lp[0].Range, new(0, 100));

        Assert.ThrowsException<KeyNotFoundException>(() => lp["item 2"]);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => lp[1]);

        // Devide the room
        lp.AddPart("item 2", 0, 1);

        Assert.AreEqual(new(0, 50), lp[0].Range);
        Assert.AreEqual(new(50, 50), lp[1].Range);

        // Devide with another
        lp.AddPart("item 3", 50, 0);

        Assert.AreEqual(new(0, 25), lp[0].Range);
        Assert.AreEqual(new(25, 25), lp[1].Range);
        Assert.AreEqual(new(50, 50), lp[2].Range);

        // Reset the scale
        lp.Range.Length = 150;

        Assert.AreEqual(new(0, 50), lp[0].Range);
        Assert.AreEqual(new(50, 50), lp[1].Range);
        Assert.AreEqual(new(100, 50), lp[2].Range);


    }
}
