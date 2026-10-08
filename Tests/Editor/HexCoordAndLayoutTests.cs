using System.Collections.Generic;
using CupkekGames.HexGrids;
using NUnit.Framework;
using UnityEngine;

namespace CupkekGames.HexGrids.Tests
{
    public class HexCoordAndLayoutTests
    {
        [Test]
        public void Distance_CountsSteps()
        {
            Assert.AreEqual(0, HexCoord.Distance(HexCoord.Zero, HexCoord.Zero));
            Assert.AreEqual(1, HexCoord.Distance(HexCoord.Zero, new HexCoord(1, -1)));
            Assert.AreEqual(3, HexCoord.Distance(new HexCoord(-1, 2), new HexCoord(2, -1)));
            Assert.AreEqual(HexCoord.Distance(new HexCoord(4, -7), new HexCoord(-2, 3)), HexCoord.Distance(new HexCoord(-2, 3), new HexCoord(4, -7)));
        }

        [TestCase(HexOrientation.PointyTop)]
        [TestCase(HexOrientation.FlatTop)]
        public void StraightDistanceSquared_IsTheCentresDistance_InSpacings_Exactly(HexOrientation orientation)
        {
            Assert.AreEqual(1, HexCoord.StraightDistanceSquared(HexCoord.Zero, new HexCoord(0, 1)), "neighbours are one spacing apart");
            Assert.AreEqual(3, HexCoord.StraightDistanceSquared(HexCoord.Zero, new HexCoord(1, 1)), "two steps off a line: root 3");
            Assert.AreEqual(4, HexCoord.StraightDistanceSquared(HexCoord.Zero, new HexCoord(2, 0)));

            var layout = new HexLayout(orientation, 0.7f, new Vector3(1f, 0f, 2f));
            var a = new HexCoord(2, -3);
            for (int q = -4; q <= 4; q++)
            {
                for (int r = -4; r <= 4; r++)
                {
                    var b = new HexCoord(q, r);
                    float metres = Vector3.Distance(layout.ToWorld(a), layout.ToWorld(b));
                    Assert.AreEqual(metres * metres / (layout.Spacing * layout.Spacing), HexCoord.StraightDistanceSquared(a, b), 1e-2f);
                    Assert.AreEqual(HexCoord.StraightDistanceSquared(a, b), HexCoord.StraightDistanceSquared(b, a));
                }
            }
        }

        [Test]
        public void TheSixNeighbours_AreOneStepAway_InAFixedOrder()
        {
            var seen = new HashSet<HexCoord>();
            for (int d = 0; d < 6; d++)
            {
                HexCoord neighbour = new HexCoord(3, -2).Neighbor((HexDirection)d);
                Assert.AreEqual(1, HexCoord.Distance(new HexCoord(3, -2), neighbour));
                Assert.IsTrue(seen.Add(neighbour), "each direction is a different neighbour");
            }

            Assert.AreEqual(new HexCoord(1, 0), HexCoord.Direction(HexDirection.East));
            Assert.AreEqual(new HexCoord(-1, 0), HexCoord.Direction(HexDirection.West));
        }

        [Test]
        public void Round_FindsTheHexThatHoldsAFraction()
        {
            Assert.AreEqual(new HexCoord(2, -1), HexCoord.Round(2.1, -0.9));
            Assert.AreEqual(new HexCoord(0, 0), HexCoord.Round(0.3, 0.3));
            Assert.AreEqual(new HexCoord(1, 0), HexCoord.Round(0.6, 0.1));
        }

        [TestCase(HexOrientation.PointyTop)]
        [TestCase(HexOrientation.FlatTop)]
        public void Layout_RoundTripsEveryHex_AndNeighboursSitOneSpacingApart(HexOrientation orientation)
        {
            var layout = new HexLayout(orientation, 1.1f, new Vector3(5f, 2f, -3f));
            for (int q = -4; q <= 4; q++)
            {
                for (int r = -4; r <= 4; r++)
                {
                    var hex = new HexCoord(q, r);
                    Vector3 world = layout.ToWorld(hex);
                    Assert.AreEqual(2f, world.y, 1e-5f, "hexes sit at the origin's height");
                    Assert.AreEqual(hex, layout.ToHex(world));
                    Assert.AreEqual(hex, layout.ToHex(world + new Vector3(0.3f, 7f, -0.2f)), "a point inside the hex finds it");
                    float gap = Vector3.Distance(world, layout.ToWorld(hex.Neighbor(HexDirection.NorthEast)));
                    Assert.AreEqual(layout.Spacing, gap, 1e-4f);
                }
            }
        }

        [Test]
        public void Layout_RefusesAZeroSize()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new HexLayout(HexOrientation.PointyTop, 0f, Vector3.zero));
        }

        [Test]
        public void Corners_SitOneSizeFromTheCentre()
        {
            var layout = new HexLayout(HexOrientation.PointyTop, 2f, Vector3.zero);
            for (int corner = 0; corner < 6; corner++)
            {
                Assert.AreEqual(2f, Vector3.Distance(layout.ToWorld(new HexCoord(1, 2)), layout.Corner(new HexCoord(1, 2), corner)), 1e-4f);
            }
        }
    }
}
