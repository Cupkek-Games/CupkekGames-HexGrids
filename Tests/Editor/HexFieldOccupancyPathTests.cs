using System.Collections.Generic;
using System.Linq;
using CupkekGames.HexGrids;
using NUnit.Framework;

namespace CupkekGames.HexGrids.Tests
{
    public class HexFieldOccupancyPathTests
    {
        [Test]
        public void Field_ListsEveryTileRowByRow_AndConvertsOffsets()
        {
            var field = new HexField(8, 4);
            Assert.AreEqual(32, field.Tiles.Count);
            Assert.AreEqual(32, field.Tiles.Distinct().Count());
            for (int i = 0; i < field.Tiles.Count; i++)
            {
                HexField.ToOffset(field.Tiles[i], out int column, out int row);
                Assert.AreEqual(i % 8, column);
                Assert.AreEqual(i / 8, row);
                Assert.IsTrue(field.Contains(field.Tiles[i]));
            }

            Assert.IsFalse(field.Contains(HexField.FromOffset(8, 0)));
            Assert.IsFalse(field.Contains(HexField.FromOffset(0, -1)));
        }

        [Test]
        public void Field_BlocksTiles_AndRefusesOffFieldOnes()
        {
            var field = new HexField(3, 3);
            HexCoord middle = HexField.FromOffset(1, 1);
            field.Block(middle);
            Assert.IsTrue(field.IsBlocked(middle));
            Assert.IsFalse(field.IsOpen(middle));
            field.Unblock(middle);
            Assert.IsTrue(field.IsOpen(middle));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => field.Block(HexField.FromOffset(5, 5)));
        }

        [Test]
        public void Occupancy_HoldsOnePerHex_AndMovesThroughReservations()
        {
            var occupancy = new HexOccupancy();
            var a = new HexCoord(0, 0);
            var b = new HexCoord(1, 0);
            occupancy.Place(1, a);
            occupancy.Place(2, new HexCoord(3, 0));

            Assert.Throws<System.InvalidOperationException>(() => occupancy.Place(3, a), "a hex holds one");
            Assert.IsTrue(occupancy.TryReserve(1, b));
            Assert.IsFalse(occupancy.IsFree(b, 2), "a reserved hex is not free for anyone else");
            Assert.IsFalse(occupancy.TryReserve(2, b));
            Assert.IsTrue(occupancy.IsFree(b, 1), "but it is for the one who reserved it");

            occupancy.CompleteMove(1);
            Assert.AreEqual(b, occupancy.PositionOf(1));
            Assert.IsTrue(occupancy.IsFree(a, 2), "the hex left behind is free");
            Assert.IsFalse(occupancy.TryGetReservation(1, out _));
            Assert.Throws<System.InvalidOperationException>(() => occupancy.CompleteMove(1), "no reservation, no move");

            occupancy.TryReserve(2, a);
            occupancy.Remove(2);
            Assert.IsTrue(occupancy.IsFree(a, 1), "removing an occupant drops its reservation");
            Assert.Throws<KeyNotFoundException>(() => occupancy.PositionOf(2));
        }

        [Test]
        public void Path_IsShortest_GoesAroundBlocks_AndEndsOnTheGoal()
        {
            var field = new HexField(8, 4);
            HexCoord start = HexField.FromOffset(0, 1);
            HexCoord goal = HexField.FromOffset(7, 1);
            var path = new List<HexCoord>();
            var pathfinder = new HexPathfinder();

            Assert.IsTrue(pathfinder.TryFindPath(field, start, goal, _ => true, path));
            Assert.AreEqual(HexCoord.Distance(start, goal), path.Count);
            Assert.AreEqual(goal, path[path.Count - 1]);

            for (int row = 0; row < 3; row++) field.Block(HexField.FromOffset(4, row));
            Assert.IsTrue(pathfinder.TryFindPath(field, start, goal, _ => true, path));
            Assert.IsFalse(path.Any(field.IsBlocked));
            Assert.Greater(path.Count, HexCoord.Distance(start, goal), "the wall forces a detour");
            for (int i = 1; i < path.Count; i++) Assert.AreEqual(1, HexCoord.Distance(path[i - 1], path[i]));

            field.Block(HexField.FromOffset(4, 3));
            Assert.IsFalse(pathfinder.TryFindPath(field, start, goal, _ => true, path), "a closed wall has no way through");
        }

        [Test]
        public void Path_ToARange_StopsAtTheFirstHexInReach_AndRespectsOccupants()
        {
            var field = new HexField(8, 4);
            var occupancy = new HexOccupancy();
            HexCoord hero = HexField.FromOffset(6, 1);
            HexCoord wolf = HexField.FromOffset(1, 1);
            occupancy.Place(1, hero);
            occupancy.Place(2, wolf);
            var path = new List<HexCoord>();
            var pathfinder = new HexPathfinder();

            Assert.IsTrue(pathfinder.TryFindPath(field, hero, wolf, 1, hex => occupancy.IsFree(hex, 1), path));
            Assert.AreEqual(1, HexCoord.Distance(path[path.Count - 1], wolf), "melee reach is one step");
            Assert.AreEqual(HexCoord.Distance(hero, wolf) - 1, path.Count);
            Assert.IsFalse(path.Contains(wolf), "never onto an occupied hex");

            Assert.IsTrue(pathfinder.TryFindPath(field, hero, wolf, 5, hex => occupancy.IsFree(hex, 1), path));
            Assert.AreEqual(0, path.Count, "already in reach: no steps");
        }

        [Test]
        public void Path_BreaksTiesTheSameWayEveryTime()
        {
            var field = new HexField(8, 4);
            var pathfinder = new HexPathfinder();
            var first = new List<HexCoord>();
            var second = new List<HexCoord>();
            HexCoord start = HexField.FromOffset(0, 0);
            HexCoord goal = HexField.FromOffset(7, 3);
            pathfinder.TryFindPath(field, start, goal, _ => true, first);
            new HexPathfinder().TryFindPath(field, start, goal, _ => true, second);
            CollectionAssert.AreEqual(first, second);
        }
    }
}
