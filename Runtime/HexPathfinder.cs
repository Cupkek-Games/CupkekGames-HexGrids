using System;
using System.Collections.Generic;

namespace CupkekGames.HexGrids
{
    /// <summary>
    /// A* over a <see cref="HexField"/>. Ties break the same way every time: the lowest
    /// estimated total, then the lowest remaining estimate, then the hex found first
    /// (neighbours are walked in <see cref="HexDirection"/> order). Reuses its buffers,
    /// so keep one per user rather than one per call.
    /// </summary>
    public sealed class HexPathfinder
    {
        private struct Node
        {
            public HexCoord Hex;
            public int Cost;
            public int Estimate;
            public int Order;
        }

        private readonly List<Node> _open = new List<Node>();
        private readonly Dictionary<HexCoord, int> _bestCost = new Dictionary<HexCoord, int>();
        private readonly Dictionary<HexCoord, HexCoord> _cameFrom = new Dictionary<HexCoord, HexCoord>();
        private readonly HashSet<HexCoord> _closed = new HashSet<HexCoord>();

        /// <summary>
        /// The shortest walk from <paramref name="start"/> to the nearest hex within
        /// <paramref name="reach"/> of <paramref name="goal"/>. <paramref name="canEnter"/>
        /// decides which hexes may be walked on (the field's open tiles are checked first);
        /// the start is always allowed. The path excludes the start and ends within reach;
        /// an empty path means the start already is. False when no such hex can be reached.
        /// </summary>
        public bool TryFindPath(HexField field, HexCoord start, HexCoord goal, int reach,
            Func<HexCoord, bool> canEnter, List<HexCoord> path)
            => TryFindPath(field, start, goal, reach, canEnter, AnyHex, path);

        /// <summary>
        /// As <see cref="TryFindPath(HexField, HexCoord, HexCoord, int, Func{HexCoord, bool}, List{HexCoord})"/>,
        /// but the walk may end only on a hex <paramref name="canStop"/> accepts (the start
        /// included): within reach of a hex it may not stop on, it walks on.
        /// </summary>
        public bool TryFindPath(HexField field, HexCoord start, HexCoord goal, int reach,
            Func<HexCoord, bool> canEnter, Func<HexCoord, bool> canStop, List<HexCoord> path)
        {
            if (reach < 0) throw new ArgumentOutOfRangeException(nameof(reach), reach, "A reach is never negative.");

            path.Clear();
            _open.Clear();
            _bestCost.Clear();
            _cameFrom.Clear();
            _closed.Clear();

            int order = 0;
            _open.Add(new Node { Hex = start, Cost = 0, Estimate = Estimate(start, goal, reach), Order = order++ });
            _bestCost[start] = 0;

            while (_open.Count > 0)
            {
                int pick = 0;
                for (int i = 1; i < _open.Count; i++)
                {
                    if (Better(_open[i], _open[pick]))
                    {
                        pick = i;
                    }
                }

                Node current = _open[pick];
                _open.RemoveAt(pick);
                if (!_closed.Add(current.Hex))
                {
                    continue;
                }

                if (HexCoord.Distance(current.Hex, goal) <= reach && canStop(current.Hex))
                {
                    for (HexCoord hex = current.Hex; hex != start; hex = _cameFrom[hex])
                    {
                        path.Add(hex);
                    }

                    path.Reverse();
                    return true;
                }

                for (int d = 0; d < 6; d++)
                {
                    HexCoord next = current.Hex.Neighbor((HexDirection)d);
                    if (_closed.Contains(next) || !field.IsOpen(next) || !canEnter(next))
                    {
                        continue;
                    }

                    int cost = current.Cost + 1;
                    if (_bestCost.TryGetValue(next, out int known) && known <= cost)
                    {
                        continue;
                    }

                    _bestCost[next] = cost;
                    _cameFrom[next] = current.Hex;
                    _open.Add(new Node { Hex = next, Cost = cost, Estimate = Estimate(next, goal, reach), Order = order++ });
                }
            }

            return false;
        }

        /// <summary>The shortest walk to <paramref name="goal"/> itself.</summary>
        public bool TryFindPath(HexField field, HexCoord start, HexCoord goal, Func<HexCoord, bool> canEnter, List<HexCoord> path)
            => TryFindPath(field, start, goal, 0, canEnter, path);

        private static readonly Func<HexCoord, bool> AnyHex = _ => true;

        // Steps still needed at the least: never more than the truth, so the walk found is a shortest one.
        private static int Estimate(HexCoord hex, HexCoord goal, int reach) => Math.Max(0, HexCoord.Distance(hex, goal) - reach);

        private static bool Better(Node a, Node b)
        {
            int fa = a.Cost + a.Estimate;
            int fb = b.Cost + b.Estimate;
            if (fa != fb) return fa < fb;
            if (a.Estimate != b.Estimate) return a.Estimate < b.Estimate;
            return a.Order < b.Order;
        }
    }
}
