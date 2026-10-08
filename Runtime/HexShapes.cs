using System;
using System.Collections.Generic;

namespace CupkekGames.HexGrids
{
    /// <summary>
    /// Sets of hexes, written into a caller's list (cleared first) in a fixed order, so a
    /// query never allocates and two runs always see the tiles the same way round.
    /// </summary>
    public static class HexShapes
    {
        private static readonly double Sqrt3 = Math.Sqrt(3.0);

        /// <summary>Every hex within <paramref name="radius"/> steps of the centre, the centre included (3r(r+1)+1 tiles).</summary>
        public static void Range(HexCoord center, int radius, List<HexCoord> results)
        {
            RequireNonNegative(radius, nameof(radius));
            results.Clear();
            for (int dq = -radius; dq <= radius; dq++)
            {
                int from = Math.Max(-radius, -dq - radius);
                int to = Math.Min(radius, -dq + radius);
                for (int dr = from; dr <= to; dr++)
                {
                    results.Add(new HexCoord(center.Q + dq, center.R + dr));
                }
            }
        }

        /// <summary>The hexes exactly <paramref name="radius"/> steps away (6r tiles; the centre alone at 0).</summary>
        public static void Ring(HexCoord center, int radius, List<HexCoord> results)
        {
            RequireNonNegative(radius, nameof(radius));
            results.Clear();
            if (radius == 0)
            {
                results.Add(center);
                return;
            }

            HexCoord hex = center + HexCoord.Direction(HexDirection.SouthWest) * radius;
            for (int side = 0; side < 6; side++)
            {
                for (int step = 0; step < radius; step++)
                {
                    results.Add(hex);
                    hex = hex.Neighbor((HexDirection)side);
                }
            }
        }

        /// <summary>The hexes a straight line from <paramref name="from"/> to <paramref name="to"/> crosses, both ends included.</summary>
        public static void Line(HexCoord from, HexCoord to, List<HexCoord> results)
        {
            results.Clear();
            int steps = HexCoord.Distance(from, to);
            if (steps == 0)
            {
                results.Add(from);
                return;
            }

            // A tiny nudge keeps the line off hex edges, so it never wavers between two tiles.
            double aq = from.Q + 1e-6, ar = from.R + 2e-6;
            double bq = to.Q + 1e-6, br = to.R + 2e-6;
            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                results.Add(HexCoord.Round(aq + (bq - aq) * t, ar + (br - ar) * t));
            }
        }

        /// <summary>
        /// The hexes within <paramref name="radius"/> steps whose direction from the origin lies
        /// within <paramref name="halfAngleDegrees"/> of the direction towards
        /// <paramref name="towards"/> (any hex but the origin), the origin excluded.
        /// Angles are measured on the hexes' own plane, so the result does not depend on
        /// the layout's size or orientation.
        /// </summary>
        public static void Cone(HexCoord origin, HexCoord towards, int radius, float halfAngleDegrees, List<HexCoord> results)
        {
            RequireNonNegative(radius, nameof(radius));
            if (towards == origin)
            {
                throw new ArgumentException("A cone needs a direction: towards must differ from the origin.", nameof(towards));
            }

            Range(origin, radius, results);
            Planar(towards - origin, out double ax, out double ay);
            double axisLength = Math.Sqrt(ax * ax + ay * ay);
            double minCos = Math.Cos(Math.PI / 180.0 * halfAngleDegrees) - 1e-9;
            int write = 0;
            for (int read = 0; read < results.Count; read++)
            {
                HexCoord hex = results[read];
                if (hex == origin)
                {
                    continue;
                }

                Planar(hex - origin, out double x, out double y);
                double cos = (x * ax + y * ay) / (Math.Sqrt(x * x + y * y) * axisLength);
                if (cos >= minCos)
                {
                    results[write++] = hex;
                }
            }

            results.RemoveRange(write, results.Count - write);
        }

        // An axial offset on a unit pointy-top plane.
        private static void Planar(HexCoord offset, out double x, out double y)
        {
            x = Sqrt3 * offset.Q + Sqrt3 / 2.0 * offset.R;
            y = 1.5 * offset.R;
        }

        private static void RequireNonNegative(int value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "A shape's radius cannot be negative.");
            }
        }
    }
}
