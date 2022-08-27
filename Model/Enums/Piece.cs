namespace SicTransit.Woodpusher.Model.Enums
{
    [Flags]
    public enum Piece
    {
        None = 0,
        Pawn = Constants.PAWN,
        Knight = Constants.KNIGHT,
        Bishop = Constants.BISHOP,
        Rook = Constants.ROOK,
        Queen = Constants.QUEEN,
        King = Constants.KING,
        White = Constants.WHITE,
        Black = Constants.BLACK,
    }
}
