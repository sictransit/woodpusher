using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class PieceOnFileMove : SimplePieceMove
    {
        private readonly int file;

        public PieceOnFileMove(Piece piece, int file, Square square) : base(piece, square)
        {
            this.file = file;
        }

        public override string ToString()
        {
            return $"from file {file}, {base.ToString()}";
        }
    }
}
