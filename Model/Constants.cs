namespace SicTransit.Woodpusher.Model
{
    public static class Constants
    {
        public const int PAWN = 0x1;
        public const int KNIGHT = 0x2;
        public const int BISHOP = 0x4;
        public const int ROOK = 0x8;
        public const int QUEEN = 0x10;
        public const int KING = 0x20;
        public const int WHITE = 0x40;
        public const int BLACK = 0x80;

        public const int PIECETYPE = PAWN | KNIGHT | BISHOP | ROOK | QUEEN | KING;
    }
}
