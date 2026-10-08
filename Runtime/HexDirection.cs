namespace CupkekGames.HexGrids
{
    /// <summary>
    /// The six neighbours of a hex, in the fixed order every query in this package walks
    /// them (so ties always break the same way). Named for a pointy-top layout, where
    /// East and West are the flat sides; on a flat-top layout the same offsets apply,
    /// only the names read rotated.
    /// </summary>
    public enum HexDirection
    {
        East = 0,
        NorthEast = 1,
        NorthWest = 2,
        West = 3,
        SouthWest = 4,
        SouthEast = 5,
    }
}
