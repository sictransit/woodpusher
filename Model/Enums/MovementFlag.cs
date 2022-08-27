namespace SicTransit.Woodpusher.Model.Enums
{
    [Flags]
    public enum MovementFlags
    {
        None = 0,
        MustTake = 1,
        EnPassant = 2,
        Promote = 4,
        CastleQueen = 8,
        CastleKing = 16
    }
}
