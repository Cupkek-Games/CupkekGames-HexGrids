using UnityEngine;

namespace CupkekGames.HexGrids
{
    /// <summary>
    /// A <see cref="HexField"/> in a scene: its size, its tile size and orientation, laid
    /// out so the field's middle sits on this transform. Draws the tiles as gizmos.
    /// The field is built on first use; change the settings in edit mode only. The layout
    /// follows the transform's position only, so a rotated or scaled transform throws.
    /// </summary>
    public class HexFieldView : MonoBehaviour
    {
        [SerializeField] private HexOrientation _orientation = HexOrientation.PointyTop;

        [Tooltip("Centre to corner, in metres: the only world number the grid has.")]
        [SerializeField, Min(0.01f)] private float _tileSize = 1f;

        [SerializeField, Min(1)] private int _columns = 8;
        [SerializeField, Min(1)] private int _rows = 4;

        [SerializeField] private Color _gizmoColor = new Color(1f, 1f, 1f, 0.35f);

        private HexField _field;

        public HexField Field => _field ??= new HexField(_columns, _rows);

        /// <summary>The layout, centred on this transform (recomputed, so it follows the transform).</summary>
        public HexLayout Layout
        {
            get
            {
                if (Quaternion.Angle(transform.rotation, Quaternion.identity) > 0.01f || (transform.lossyScale - Vector3.one).sqrMagnitude > 1e-6f)
                {
                    throw new System.InvalidOperationException($"[HexFieldView] '{name}' is rotated or scaled; a hex field follows its position only. Keep it and its parents unrotated and unscaled.");
                }

                var unit = new HexLayout(_orientation, _tileSize, Vector3.zero);
                Vector3 first = unit.ToWorld(HexField.FromOffset(0, 0));
                Vector3 last = unit.ToWorld(HexField.FromOffset(_columns - 1, _rows - 1));
                Vector3 lastOfFirstRow = unit.ToWorld(HexField.FromOffset(_columns - 1, 0));
                float minX = Mathf.Min(first.x, unit.ToWorld(HexField.FromOffset(0, Mathf.Min(1, _rows - 1))).x);
                float maxX = Mathf.Max(lastOfFirstRow.x, last.x);
                var middle = new Vector3((minX + maxX) * 0.5f, 0f, (first.z + last.z) * 0.5f);
                return unit.WithOrigin(transform.position - middle);
            }
        }

        public Vector3 ToWorld(HexCoord hex) => Layout.ToWorld(hex);

        public HexCoord ToHex(Vector3 world) => Layout.ToHex(world);

        private void OnDrawGizmos()
        {
            HexLayout layout = Layout;
            var field = new HexField(_columns, _rows);
            Gizmos.color = _gizmoColor;
            foreach (HexCoord hex in field.Tiles)
            {
                for (int corner = 0; corner < 6; corner++)
                {
                    Gizmos.DrawLine(layout.Corner(hex, corner), layout.Corner(hex, (corner + 1) % 6));
                }
            }
        }
    }
}
