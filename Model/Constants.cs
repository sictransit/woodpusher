namespace SicTransit.Woodpusher.Model
{
    public static class Constants
    {
        public const int Pawn = 0x1;
        public const int Knight = 0x2;
        public const int Bishop = 0x4;
        public const int Rook = 0x8;
        public const int Queen = 0x10;
        public const int King = 0x20;

        public const int White = 0x40;
        public const int Black = 0x80;

        public const int PieceTypeMask = Pawn | Knight | Bishop | Rook | Queen | King;
    }
}
