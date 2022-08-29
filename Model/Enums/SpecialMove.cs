namespace SicTransit.Woodpusher.Model.Enums
{
    [Flags]
    public enum SpecialMove
    {
        None = 0,
        MustTake = 1,
        CannotTake = 2,
        EnPassant = 4,
        Promote = 8,
        CastleQueen = 16,
        CastleKing = 32
    }
}
