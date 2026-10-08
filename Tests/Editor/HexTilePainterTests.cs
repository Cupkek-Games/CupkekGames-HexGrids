using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CupkekGames.HexGrids.Tests
{
    public class HexTilePainterTests
    {
        private GameObject _root;
        private HexFieldView _field;
        private HexTilePainter _painter;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("HexTilePainterTests");
            _field = _root.AddComponent<HexFieldView>();
            _painter = _root.AddComponent<HexTilePainter>();
            var so = new SerializedObject(_painter);
            so.FindProperty("_field").objectReferenceValue = _field;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Mesh mesh = _root.GetComponent<MeshFilter>().sharedMesh;
            if (mesh != null) UnityEngine.Object.DestroyImmediate(mesh);
            UnityEngine.Object.DestroyImmediate(_root);
        }

        [Test]
        public void Paint_RefusesAHexOffTheField_AndAPainterWithNoField()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _painter.Paint(HexField.FromOffset(30, 0), Color.white, Color.white));

            var bare = new GameObject("Bare").AddComponent<HexTilePainter>();
            try
            {
                Assert.Throws<InvalidOperationException>(() => bare.Paint(HexCoord.Zero, Color.white, Color.white));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bare.gameObject);
            }
        }

        [Test]
        public void TryGetPaint_ReadsWhatIsPainted_EraseAndClearForget()
        {
            HexCoord a = HexField.FromOffset(1, 1), b = HexField.FromOffset(2, 1);
            _painter.Paint(a, Color.red, Color.white);
            _painter.Paint(b, Color.clear, Color.blue);
            Assert.IsTrue(_painter.TryGetPaint(a, out HexTileStyle style));
            Assert.AreEqual(Color.red, style.Fill);

            _painter.Erase(a);
            Assert.IsFalse(_painter.TryGetPaint(a, out _));
            _painter.Clear();
            Assert.IsFalse(_painter.TryGetPaint(b, out _));
        }

        [Test]
        public void Apply_DrawsInTheFieldsTileOrder()
        {
            _painter.Paint(HexField.FromOffset(3, 2), Color.white, Color.clear);
            _painter.Paint(HexField.FromOffset(0, 0), Color.white, Color.clear);
            _painter.Apply();
            Mesh mesh = _root.GetComponent<MeshFilter>().sharedMesh;
            Vector3 first = mesh.vertices[0];
            Vector3 centre = _field.ToWorld(HexField.FromOffset(0, 0)) - _root.transform.position;
            Assert.AreEqual(centre.x, first.x, 1e-4f, "The field's first tile is drawn first, whatever order it was painted in.");
            Assert.AreEqual(centre.z, first.z, 1e-4f);
        }
    }
}
