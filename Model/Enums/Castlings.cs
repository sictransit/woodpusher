namespace SicTransit.Woodpusher.Model.Enums
{
    [Flags]
    public enum Castlings
    {
        None = 0,
        WhiteKingside = 1,
        WhiteQueenside = 2,
        BlackKingside = 4,
        BlackQueenside = 8,
    }
}
