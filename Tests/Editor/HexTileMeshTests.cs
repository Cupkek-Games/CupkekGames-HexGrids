using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CupkekGames.HexGrids.Tests
{
    public class HexTileMeshTests
    {
        private static readonly HexLayout Layout = new HexLayout(HexOrientation.PointyTop, 1f, Vector3.zero);

        private static Mesh Build(IReadOnlyList<HexTileStyle> tiles, float opacity = 1f)
        {
            var mesh = new Mesh();
            HexTileMesh.Build(mesh, Layout, tiles, 0.9f, 0.05f, 0.85f, 0.02f, opacity,
                new List<Vector3>(), new List<Color>(), new List<int>());
            return mesh;
        }

        [Test]
        public void AFillIsAHexagon_ARingIsSixQuads()
        {
            Mesh both = Build(new[] { new HexTileStyle(HexCoord.Zero, Color.white, Color.white) });
            Assert.AreEqual(7 + 12, both.vertexCount, "Fill: the centre and six corners; ring: six outer and six inner corners.");
            Assert.AreEqual((6 + 12) * 3, both.triangles.Length, "Six fill triangles and two per ring side.");

            Mesh ringOnly = Build(new[] { new HexTileStyle(HexCoord.Zero, Color.clear, Color.white) });
            Assert.AreEqual(12, ringOnly.vertexCount, "A clear fill is skipped.");
            Object.DestroyImmediate(both);
            Object.DestroyImmediate(ringOnly);
        }

        [Test]
        public void TheMeshFacesUp_AndSitsOnTheLift()
        {
            Mesh mesh = Build(new[] { new HexTileStyle(HexCoord.Zero, Color.white, Color.clear) });
            mesh.RecalculateNormals();
            foreach (Vector3 normal in mesh.normals) Assert.Greater(normal.y, 0.99f);
            foreach (Vector3 vertex in mesh.vertices) Assert.AreEqual(0.02f, vertex.y, 1e-5f);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Opacity_ScalesAlpha_AndZeroDrawsNothing()
        {
            var tile = new[] { new HexTileStyle(HexCoord.Zero, new Color(1f, 1f, 1f, 0.5f), Color.clear) };
            Mesh half = Build(tile, 0.5f);
            Assert.AreEqual(0.25f, half.colors[0].a, 1e-5f);
            Mesh none = Build(tile, 0f);
            Assert.AreEqual(0, none.vertexCount);
            Object.DestroyImmediate(half);
            Object.DestroyImmediate(none);
        }
    }
}
