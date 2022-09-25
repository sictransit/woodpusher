using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Interfaces;
using SicTransit.Woodpusher.Parsing.Exceptions;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class PieceOnFileMove : PgnMove
    {
        private readonly PieceType pieceType;
        private readonly int file;
        private readonly Square square;
        private readonly PieceType promotionType;

        public PieceOnFileMove(string raw, PieceType pieceType, int file, Square square, PieceType promotionType) : base(raw)
        {
            this.pieceType = pieceType;
            this.file = file;
            this.square = square;
            this.promotionType = promotionType;
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] {pieceType} from file {file} to {square}";
        }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var positions = board.GetPositions(board.ActiveColor, pieceType);

            foreach (var position in positions.Where(p => p.Square.File == file))
            {
                var move = engine.Board.GetLegalMoves(position).SingleOrDefault(m => m.Target.Equals(square) && m.PromotionType == promotionType);

                if (move != null)
                {
                    return move;
                }
            }

            throw new PgnParsingException(Raw, "unable to a legal move to match");
        }
    }
}
