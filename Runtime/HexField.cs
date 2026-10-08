using System;
using System.Collections.Generic;

namespace CupkekGames.HexGrids
{
    /// <summary>
    /// A rectangular board of <see cref="Columns"/> by <see cref="Rows"/> hexes (offset
    /// rows, odd rows shifted half a tile right for a pointy-top layout), with blocked
    /// tiles. <see cref="Tiles"/> lists every tile row by row, the order every scan uses.
    /// </summary>
    public sealed class HexField
    {
        private readonly List<HexCoord> _tiles = new List<HexCoord>();
        private readonly HashSet<HexCoord> _blocked = new HashSet<HexCoord>();

        public HexField(int columns, int rows)
        {
            if (columns <= 0 || rows <= 0)
            {
                throw new ArgumentOutOfRangeException(columns <= 0 ? nameof(columns) : nameof(rows), "A field needs at least one column and one row.");
            }

            Columns = columns;
            Rows = rows;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    _tiles.Add(FromOffset(column, row));
                }
            }
        }

        public int Columns { get; }
        public int Rows { get; }

        /// <summary>Every tile, row by row, left to right.</summary>
        public IReadOnlyList<HexCoord> Tiles => _tiles;

        /// <summary>The hex at a column and row (odd rows shifted right).</summary>
        public static HexCoord FromOffset(int column, int row) => new HexCoord(column - (row - (row & 1)) / 2, row);

        /// <summary>The column and row of a hex.</summary>
        public static void ToOffset(HexCoord hex, out int column, out int row)
        {
            row = hex.R;
            column = hex.Q + (hex.R - (hex.R & 1)) / 2;
        }

        public bool Contains(HexCoord hex)
        {
            ToOffset(hex, out int column, out int row);
            return column >= 0 && column < Columns && row >= 0 && row < Rows;
        }

        public bool IsBlocked(HexCoord hex) => _blocked.Contains(hex);

        /// <summary>On the field and not blocked.</summary>
        public bool IsOpen(HexCoord hex) => Contains(hex) && !_blocked.Contains(hex);

        public void Block(HexCoord hex)
        {
            RequireOnField(hex);
            _blocked.Add(hex);
        }

        public void Unblock(HexCoord hex)
        {
            RequireOnField(hex);
            _blocked.Remove(hex);
        }

        /// <summary>The tile at (or just before) the middle column and row.</summary>
        public HexCoord Center => FromOffset((Columns - 1) / 2, (Rows - 1) / 2);

        private void RequireOnField(HexCoord hex)
        {
            if (!Contains(hex))
            {
                throw new ArgumentOutOfRangeException(nameof(hex), hex, $"Not on this {Columns}x{Rows} field.");
            }
        }
    }
}
