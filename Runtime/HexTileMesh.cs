using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.HexGrids
{
    /// <summary>How one hex is drawn: a flat fill and a ring, each with its own colour (alpha 0 skips it).</summary>
    public readonly struct HexTileStyle
    {
        public readonly HexCoord Hex;
        /// <summary>The fill's colour at its edge (and everywhere, unless <see cref="FillCentre"/> differs).</summary>
        public readonly Color Fill;
        public readonly Color Ring;
        /// <summary>The fill's colour at the hex's centre, blending out to <see cref="Fill"/> at its edge.</summary>
        public readonly Color FillCentre;

        public HexTileStyle(HexCoord hex, Color fill, Color ring)
            : this(hex, fill, ring, fill)
        {
        }

        /// <summary>
        /// A hex whose fill blends from <paramref name="fillCentre"/> at its centre to <paramref name="fill"/> at its
        /// edge: a see-through centre gives a soft fill that gathers at the ring.
        /// </summary>
        public HexTileStyle(HexCoord hex, Color fill, Color ring, Color fillCentre)
        {
            Hex = hex;
            Fill = fill;
            Ring = ring;
            FillCentre = fillCentre;
        }
    }

    /// <summary>
    /// Writes hexes into one mesh, each drawn as a flat fill and a ring inset from its edge
    /// (neighbouring rings stay apart, as board games draw their hexes), coloured by vertex
    /// colour and lying on the layout's plane. Rebuild when the hexes or their colours change.
    /// </summary>
    public static class HexTileMesh
    {
        /// <param name="ringInset">Where the ring's middle sits, as a share of the way from centre to corner.</param>
        /// <param name="ringWidth">The ring's width in metres.</param>
        /// <param name="fillInset">How far the fill reaches, as a share of the way from centre to corner.</param>
        /// <param name="lift">Metres above the layout's plane.</param>
        /// <param name="opacity">Multiplies every colour's alpha (for fades).</param>
        public static void Build(Mesh mesh, HexLayout layout, IReadOnlyList<HexTileStyle> tiles,
            float ringInset, float ringWidth, float fillInset, float lift, float opacity,
            List<Vector3> vertices, List<Color> colors, List<int> triangles)
        {
            vertices.Clear();
            colors.Clear();
            triangles.Clear();
            float outer = layout.Size * ringInset + ringWidth * 0.5f;
            float inner = Mathf.Max(0f, layout.Size * ringInset - ringWidth * 0.5f);
            float fill = layout.Size * fillInset;
            for (int t = 0; t < tiles.Count; t++)
            {
                HexTileStyle style = tiles[t];
                Vector3 centre = layout.ToWorld(style.Hex);
                centre.y += lift;

                Color fillColor = Faded(style.Fill, opacity);
                Color centreColor = Faded(style.FillCentre, opacity);
                if (fillColor.a > 0f || centreColor.a > 0f)
                {
                    int first = vertices.Count;
                    vertices.Add(centre);
                    colors.Add(centreColor);
                    for (int corner = 0; corner < 6; corner++)
                    {
                        vertices.Add(centre + Direction(layout, style.Hex, corner) * fill);
                        colors.Add(fillColor);
                    }

                    for (int corner = 0; corner < 6; corner++)
                    {
                        // Corners run counter-clockwise seen from above; Unity's front faces are clockwise.
                        triangles.Add(first);
                        triangles.Add(first + 1 + (corner + 1) % 6);
                        triangles.Add(first + 1 + corner);
                    }
                }

                Color ringColor = Faded(style.Ring, opacity);
                if (ringColor.a > 0f)
                {
                    int first = vertices.Count;
                    for (int corner = 0; corner < 6; corner++)
                    {
                        Vector3 direction = Direction(layout, style.Hex, corner);
                        vertices.Add(centre + direction * outer);
                        vertices.Add(centre + direction * inner);
                        colors.Add(ringColor);
                        colors.Add(ringColor);
                    }

                    for (int corner = 0; corner < 6; corner++)
                    {
                        int o0 = first + corner * 2;
                        int i0 = o0 + 1;
                        int o1 = first + (corner + 1) % 6 * 2;
                        int i1 = o1 + 1;
                        triangles.Add(o0);
                        triangles.Add(i0);
                        triangles.Add(o1);
                        triangles.Add(i0);
                        triangles.Add(i1);
                        triangles.Add(o1);
                    }
                }
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        // A unit vector from a hex's centre towards one of its corners.
        private static Vector3 Direction(HexLayout layout, HexCoord hex, int corner)
            => (layout.Corner(hex, corner) - layout.ToWorld(hex)) / layout.Size;

        private static Color Faded(Color color, float opacity) => new Color(color.r, color.g, color.b, color.a * opacity);
    }
}
