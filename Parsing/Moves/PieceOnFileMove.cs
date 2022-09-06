using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class PieceOnFileMove : PgnMove
    {
        private readonly PieceType pieceType;
        private readonly int file;
        private readonly Square square;

        public PieceOnFileMove(PieceType pieceType, int file, Square square)
        {
            this.pieceType = pieceType;
            this.file = file;
            this.square = square;
        }

        public override string ToString()
        {
            return $"{pieceType} from file {file} to {square}";
        }

        protected override Move CreateMove(IEngine engine)
        {
            throw new NotImplementedException();
        }
    }
}
