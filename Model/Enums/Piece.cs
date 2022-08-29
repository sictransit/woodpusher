namespace SicTransit.Woodpusher.Model.Enums
{
    [Flags]
    public enum Piece
    {
        None = 0,
        Pawn = Constants.Pawn,
        Knight = Constants.Knight,
        Bishop = Constants.Bishop,
        Rook = Constants.Rook,
        Queen = Constants.Queen,
        King = Constants.King,
        White = Constants.White,
        Black = Constants.Black,
    }
}
