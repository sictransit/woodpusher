namespace SicTransit.Woodpusher.Model.Enums
{
    [Flags]
    public enum SpecialMove
    {
        None = 0,
        PawnTakes = 1,
        PawnMoves = 2,
        PawnTakesEnPassant = 4,
        PawnPromotes = 8,
        CastleQueen = 16,
        CastleKing = 32
    }
}
