using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.HexGrids
{
    /// <summary>
    /// Paints hexes of a <see cref="HexFieldView"/> on the ground in play: any set of hexes,
    /// each with its own fill and ring colour, in one mesh. Paint, then <see cref="Apply"/>;
    /// <see cref="Opacity"/> fades everything at once. Give it a transparent, vertex-coloured
    /// material; keep the object unrotated and unscaled, as the field's layout is.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexTilePainter : MonoBehaviour
    {
        [SerializeField] private HexFieldView _field;

        [Tooltip("Where a hex's ring sits, as a share of the way from its centre to a corner.")]
        [SerializeField, Range(0.5f, 1f)] private float _ringInset = 0.92f;

        [Tooltip("The ring's width in metres.")]
        [SerializeField, Min(0.001f)] private float _ringWidth = 0.05f;

        [Tooltip("How far a hex's fill reaches, as a share of the way from its centre to a corner.")]
        [SerializeField, Range(0f, 1f)] private float _fillInset = 0.9f;

        [Tooltip("Metres above the field's plane, so the paint clears the ground.")]
        [SerializeField] private float _lift = 0.02f;

        private readonly Dictionary<HexCoord, HexTileStyle> _painted = new Dictionary<HexCoord, HexTileStyle>();
        private readonly List<HexTileStyle> _ordered = new List<HexTileStyle>();
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Color> _colors = new List<Color>();
        private readonly List<int> _triangles = new List<int>();
        private Mesh _mesh;
        private float _opacity = 1f;

        /// <summary>Metres above the field's plane; set it before painting.</summary>
        public float Lift
        {
            get => _lift;
            set => _lift = value;
        }

        /// <summary>The field it paints on (set before the first paint when made in code).</summary>
        public HexFieldView Field
        {
            get => _field;
            set => _field = value;
        }

        /// <summary>Multiplies every painted colour's alpha; setting it redraws.</summary>
        public float Opacity
        {
            get => _opacity;
            set
            {
                float clamped = Mathf.Clamp01(value);
                if (Mathf.Approximately(clamped, _opacity)) return;
                _opacity = clamped;
                Apply();
            }
        }

        /// <summary>Forgets every painted hex (call <see cref="Apply"/> to redraw).</summary>
        public void Clear() => _painted.Clear();

        /// <summary>Paints one hex of the field (replacing its earlier paint); <see cref="Apply"/> draws it.</summary>
        public void Paint(HexCoord hex, Color fill, Color ring)
        {
            if (_field == null) throw new InvalidOperationException($"[HexTilePainter] '{name}' has no HexFieldView to paint on.");
            if (!_field.Field.Contains(hex)) throw new ArgumentOutOfRangeException(nameof(hex), hex, $"[HexTilePainter] '{name}': {hex} is off the field.");
            _painted[hex] = new HexTileStyle(hex, fill, ring);
        }

        /// <summary>What a hex is painted with; false when it is not painted.</summary>
        public bool TryGetPaint(HexCoord hex, out HexTileStyle style) => _painted.TryGetValue(hex, out style);

        /// <summary>Stops painting one hex; <see cref="Apply"/> redraws.</summary>
        public void Erase(HexCoord hex) => _painted.Remove(hex);

        /// <summary>Rebuilds the mesh from what is painted.</summary>
        public void Apply()
        {
            if (_field == null) throw new InvalidOperationException($"[HexTilePainter] '{name}' has no HexFieldView to paint on.");

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "Hex Tile Paint" };
                _mesh.MarkDynamic();
                GetComponent<MeshFilter>().sharedMesh = _mesh;
            }

            // Drawn in the field's tile order, so a redraw never reshuffles the triangles.
            _ordered.Clear();
            foreach (HexCoord hex in _field.Field.Tiles)
            {
                if (_painted.TryGetValue(hex, out HexTileStyle style)) _ordered.Add(style);
            }

            HexLayout layout = _field.Layout;
            HexTileMesh.Build(_mesh, layout.WithOrigin(layout.Origin - transform.position), _ordered,
                _ringInset, _ringWidth, _fillInset, _lift, _opacity, _vertices, _colors, _triangles);
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }
    }
}
