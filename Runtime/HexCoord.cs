using System;

namespace CupkekGames.HexGrids
{
    /// <summary>
    /// A hex in axial coordinates (q, r); the third cube coordinate is s = -q - r.
    /// Distances count tiles. Integer only, so every result is exact and repeatable.
    /// </summary>
    [Serializable]
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        /// <summary>The axial offsets of the six neighbours, in <see cref="HexDirection"/> order.</summary>
        private static readonly HexCoord[] DirectionOffsets =
        {
            new HexCoord(1, 0),
            new HexCoord(1, -1),
            new HexCoord(0, -1),
            new HexCoord(-1, 0),
            new HexCoord(-1, 1),
            new HexCoord(0, 1),
        };

        public static readonly HexCoord Zero = new HexCoord(0, 0);

        public readonly int Q;
        public readonly int R;

        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        /// <summary>The third cube coordinate.</summary>
        public int S => -Q - R;

        /// <summary>The offset one step in <paramref name="direction"/>.</summary>
        public static HexCoord Direction(HexDirection direction) => DirectionOffsets[(int)direction];

        /// <summary>The neighbour one step in <paramref name="direction"/>.</summary>
        public HexCoord Neighbor(HexDirection direction) => this + Direction(direction);

        /// <summary>The number of steps between two hexes.</summary>
        public static int Distance(HexCoord a, HexCoord b)
        {
            int dq = a.Q - b.Q;
            int dr = a.R - b.R;
            int ds = a.S - b.S;
            return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
        }

        public int DistanceTo(HexCoord other) => Distance(this, other);

        /// <summary>
        /// The straight-line distance between two hexes' centres, squared, in tile spacings
        /// (neighbours are 1 apart): exact on either orientation, so ties compare exactly.
        /// </summary>
        public static int StraightDistanceSquared(HexCoord a, HexCoord b)
        {
            int dq = a.Q - b.Q;
            int dr = a.R - b.R;
            return dq * dq + dq * dr + dr * dr;
        }

        /// <summary>
        /// The hex that holds the fractional axial position (q, r): cube rounding, with the
        /// coordinate that rounded furthest recomputed from the other two.
        /// </summary>
        public static HexCoord Round(double q, double r)
        {
            double s = -q - r;
            double rq = Math.Round(q, MidpointRounding.AwayFromZero);
            double rr = Math.Round(r, MidpointRounding.AwayFromZero);
            double rs = Math.Round(s, MidpointRounding.AwayFromZero);
            double dq = Math.Abs(rq - q);
            double dr = Math.Abs(rr - r);
            double ds = Math.Abs(rs - s);

            if (dq > dr && dq > ds)
            {
                rq = -rr - rs;
            }
            else if (dr > ds)
            {
                rr = -rq - rs;
            }

            return new HexCoord((int)rq, (int)rr);
        }

        public static HexCoord operator +(HexCoord a, HexCoord b) => new HexCoord(a.Q + b.Q, a.R + b.R);
        public static HexCoord operator -(HexCoord a, HexCoord b) => new HexCoord(a.Q - b.Q, a.R - b.R);
        public static HexCoord operator *(HexCoord a, int k) => new HexCoord(a.Q * k, a.R * k);
        public static bool operator ==(HexCoord a, HexCoord b) => a.Q == b.Q && a.R == b.R;
        public static bool operator !=(HexCoord a, HexCoord b) => !(a == b);

        public bool Equals(HexCoord other) => this == other;
        public override bool Equals(object obj) => obj is HexCoord other && this == other;
        public override int GetHashCode() => unchecked((Q * 397) ^ R);
        public override string ToString() => $"({Q}, {R})";
    }
}
