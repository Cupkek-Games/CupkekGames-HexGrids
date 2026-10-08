using System;
using UnityEngine;

namespace CupkekGames.HexGrids
{
    /// <summary>Which way the hexes point: corners up (rows run along X) or flat sides up.</summary>
    public enum HexOrientation
    {
        PointyTop = 0,
        FlatTop = 1,
    }

    /// <summary>
    /// Where hexes sit in the world: on the XZ plane at <see cref="Origin"/>'s height,
    /// <see cref="Size"/> from a centre to a corner. The size is the one world number a
    /// grid has; everything else counts tiles. Axial r grows along +Z.
    /// </summary>
    [Serializable]
    public readonly struct HexLayout
    {
        private static readonly double Sqrt3 = Math.Sqrt(3.0);

        public readonly HexOrientation Orientation;
        public readonly float Size;
        public readonly Vector3 Origin;

        public HexLayout(HexOrientation orientation, float size, Vector3 origin)
        {
            if (size <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, "A hex needs a positive size.");
            }

            Orientation = orientation;
            Size = size;
            Origin = origin;
        }

        /// <summary>The distance between two neighbouring centres.</summary>
        public float Spacing => (float)(Sqrt3 * Size);

        /// <summary>The world position of a hex's centre.</summary>
        public Vector3 ToWorld(HexCoord hex)
        {
            double x;
            double z;
            if (Orientation == HexOrientation.PointyTop)
            {
                x = Size * (Sqrt3 * hex.Q + Sqrt3 / 2.0 * hex.R);
                z = Size * (1.5 * hex.R);
            }
            else
            {
                x = Size * (1.5 * hex.Q);
                z = Size * (Sqrt3 / 2.0 * hex.Q + Sqrt3 * hex.R);
            }

            return new Vector3(Origin.x + (float)x, Origin.y, Origin.z + (float)z);
        }

        /// <summary>The hex under a world position (its height is ignored).</summary>
        public HexCoord ToHex(Vector3 world)
        {
            double x = (world.x - Origin.x) / Size;
            double z = (world.z - Origin.z) / Size;
            double q;
            double r;
            if (Orientation == HexOrientation.PointyTop)
            {
                q = Sqrt3 / 3.0 * x - 1.0 / 3.0 * z;
                r = 2.0 / 3.0 * z;
            }
            else
            {
                q = 2.0 / 3.0 * x;
                r = -1.0 / 3.0 * x + Sqrt3 / 3.0 * z;
            }

            return HexCoord.Round(q, r);
        }

        /// <summary>One of a hex's six corners, 0 to 5, counter-clockwise from the right.</summary>
        public Vector3 Corner(HexCoord hex, int corner)
        {
            double degrees = 60.0 * corner + (Orientation == HexOrientation.PointyTop ? 30.0 : 0.0);
            double radians = Math.PI / 180.0 * degrees;
            Vector3 centre = ToWorld(hex);
            return new Vector3(centre.x + (float)(Size * Math.Cos(radians)), centre.y, centre.z + (float)(Size * Math.Sin(radians)));
        }

        /// <summary>The same layout moved to another origin.</summary>
        public HexLayout WithOrigin(Vector3 origin) => new HexLayout(Orientation, Size, origin);
    }
}
