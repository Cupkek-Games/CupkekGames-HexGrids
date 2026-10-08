using System.Collections.Generic;
using System.Linq;
using CupkekGames.HexGrids;
using NUnit.Framework;

namespace CupkekGames.HexGrids.Tests
{
    public class HexShapesTests
    {
        private readonly List<HexCoord> _a = new List<HexCoord>();
        private readonly List<HexCoord> _b = new List<HexCoord>();

        [TestCase(0, 1)]
        [TestCase(1, 7)]
        [TestCase(2, 19)]
        [TestCase(3, 37)]
        public void Range_HoldsEveryHexWithinTheRadius(int radius, int count)
        {
            var center = new HexCoord(2, -1);
            HexShapes.Range(center, radius, _a);
            Assert.AreEqual(count, _a.Count);
            Assert.AreEqual(count, _a.Distinct().Count());
            Assert.IsTrue(_a.All(hex => HexCoord.Distance(center, hex) <= radius));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void Ring_HoldsTheHexesExactlyThatFar(int radius)
        {
            HexShapes.Ring(HexCoord.Zero, radius, _a);
            Assert.AreEqual(6 * radius, _a.Count);
            Assert.AreEqual(6 * radius, _a.Distinct().Count());
            Assert.IsTrue(_a.All(hex => HexCoord.Distance(HexCoord.Zero, hex) == radius));
        }

        [Test]
        public void Line_IsConnected_AndRunsEndToEnd()
        {
            var from = new HexCoord(-2, 3);
            var to = new HexCoord(4, -2);
            HexShapes.Line(from, to, _a);
            Assert.AreEqual(HexCoord.Distance(from, to) + 1, _a.Count);
            Assert.AreEqual(from, _a[0]);
            Assert.AreEqual(to, _a[_a.Count - 1]);
            for (int i = 1; i < _a.Count; i++)
            {
                Assert.AreEqual(1, HexCoord.Distance(_a[i - 1], _a[i]), "every step is to a neighbour");
            }
        }

        [Test]
        public void Cone_KeepsTheHexesAlongItsAxis_AndDropsTheOnesBehind()
        {
            HexShapes.Cone(HexCoord.Zero, new HexCoord(1, 0), 3, 30f, _a);
            Assert.Contains(new HexCoord(1, 0), _a);
            Assert.Contains(new HexCoord(3, 0), _a);
            Assert.IsFalse(_a.Contains(HexCoord.Zero), "the origin is not in its own cone");
            Assert.IsFalse(_a.Contains(new HexCoord(-1, 0)), "nothing behind");
            Assert.IsFalse(_a.Contains(new HexCoord(0, 1)), "60 degrees off the axis is outside a 30 degree half-angle");
            Assert.IsTrue(_a.All(hex => HexCoord.Distance(HexCoord.Zero, hex) <= 3));

            HexShapes.Cone(HexCoord.Zero, new HexCoord(1, 0), 3, 180f, _b);
            Assert.AreEqual(36, _b.Count, "a full half-angle is the whole range but the origin");
        }

        [Test]
        public void Shapes_ComeOutInTheSameOrderEveryTime()
        {
            HexShapes.Range(new HexCoord(1, 1), 2, _a);
            HexShapes.Range(new HexCoord(1, 1), 2, _b);
            CollectionAssert.AreEqual(_a, _b);
            HexShapes.Cone(HexCoord.Zero, new HexCoord(2, -1), 4, 45f, _a);
            HexShapes.Cone(HexCoord.Zero, new HexCoord(2, -1), 4, 45f, _b);
            CollectionAssert.AreEqual(_a, _b);
        }

        [Test]
        public void ANegativeRadius_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => HexShapes.Range(HexCoord.Zero, -1, _a));
            Assert.Throws<System.ArgumentException>(() => HexShapes.Cone(HexCoord.Zero, HexCoord.Zero, 2, 30f, _a));
        }
    }
}
