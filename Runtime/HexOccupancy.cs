using System;
using System.Collections.Generic;

namespace CupkekGames.HexGrids
{
    /// <summary>
    /// Who stands where, one occupant per hex, by integer id. A mover first reserves the
    /// next hex (no one else may take or reserve it), then completes the move, which
    /// frees the hex it left. Wrong calls throw; nothing fails quietly.
    /// </summary>
    public sealed class HexOccupancy
    {
        private readonly Dictionary<HexCoord, int> _occupant = new Dictionary<HexCoord, int>();
        private readonly Dictionary<int, HexCoord> _position = new Dictionary<int, HexCoord>();
        private readonly Dictionary<HexCoord, int> _reservedBy = new Dictionary<HexCoord, int>();
        private readonly Dictionary<int, HexCoord> _reservation = new Dictionary<int, HexCoord>();

        public int Count => _position.Count;

        public bool Contains(int id) => _position.ContainsKey(id);

        public HexCoord PositionOf(int id)
        {
            if (!_position.TryGetValue(id, out HexCoord hex))
            {
                throw new KeyNotFoundException($"Occupant {id} is not on the grid.");
            }

            return hex;
        }

        public bool TryGetOccupant(HexCoord hex, out int id) => _occupant.TryGetValue(hex, out id);

        public bool TryGetReservation(int id, out HexCoord hex) => _reservation.TryGetValue(id, out hex);

        /// <summary>Nobody stands there and nobody but <paramref name="forId"/> has reserved it.</summary>
        public bool IsFree(HexCoord hex, int forId)
        {
            if (_occupant.TryGetValue(hex, out int standing) && standing != forId)
            {
                return false;
            }

            return !_reservedBy.TryGetValue(hex, out int reserving) || reserving == forId;
        }

        /// <summary>Puts a new occupant on a free hex.</summary>
        public void Place(int id, HexCoord hex)
        {
            if (_position.ContainsKey(id))
            {
                throw new InvalidOperationException($"Occupant {id} is already on the grid at {_position[id]}.");
            }

            if (!IsFree(hex, id))
            {
                throw new InvalidOperationException($"{hex} is taken; occupant {id} cannot be placed there.");
            }

            _occupant[hex] = id;
            _position[id] = hex;
        }

        /// <summary>Takes an occupant off the grid, with any reservation it holds.</summary>
        public void Remove(int id)
        {
            HexCoord hex = PositionOf(id);
            CancelReservation(id);
            _occupant.Remove(hex);
            _position.Remove(id);
        }

        /// <summary>Claims the hex an occupant moves to next; false if someone has it.</summary>
        public bool TryReserve(int id, HexCoord hex)
        {
            PositionOf(id);
            if (!IsFree(hex, id))
            {
                return false;
            }

            CancelReservation(id);
            _reservedBy[hex] = id;
            _reservation[id] = hex;
            return true;
        }

        public void CancelReservation(int id)
        {
            if (_reservation.TryGetValue(id, out HexCoord hex))
            {
                _reservation.Remove(id);
                _reservedBy.Remove(hex);
            }
        }

        /// <summary>Moves an occupant onto the hex it reserved, freeing the one it left.</summary>
        public void CompleteMove(int id)
        {
            HexCoord from = PositionOf(id);
            if (!_reservation.TryGetValue(id, out HexCoord to))
            {
                throw new InvalidOperationException($"Occupant {id} has no reservation to move onto.");
            }

            CancelReservation(id);
            _occupant.Remove(from);
            _occupant[to] = id;
            _position[id] = to;
        }
    }
}
