namespace SicTransit.Woodpusher.Model.Check
{
    public struct CheckMask
    {
        public ulong Pawn { get; init; }
        public ulong Rook { get; init; }
        public ulong Knight { get; init; }
        public ulong Bishop { get; init; }
        public ulong Queen { get; init; }

        public CheckMask(ulong pawn, ulong rook, ulong knight, ulong bishop, ulong queen)
        {
            Pawn = pawn;
            Rook = rook;
            Knight = knight;
            Bishop = bishop;
            Queen = queen;
        }
    }
}
