namespace SicTransit.Woodpusher.Model.Enums;

[Flags]
public enum Pieces : int
{
    None = 0,
    //    Black = 0,
    Pawn = 1 << 8,
    Knight = 1 << 9,
    Bishop = 1 << 10,
    Rook = 1 << 11,
    Queen = 1 << 12,
    King = 1 << 13,
    White = 1 << 14
}

