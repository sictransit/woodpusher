using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class SpecificPieceMove : PgnMove
    {
        private readonly Piece pieceType;
        private readonly Square position;
        private readonly Square square;
        private readonly Piece promotionType;

        public SpecificPieceMove(Piece pieceType, Square position, Square square, Piece promotionType)
        {
            this.pieceType = pieceType;
            this.position = position;
            this.square = square;
            this.promotionType = promotionType;
        }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var pieces = board.GetPieces(board.ActiveColor, pieceType);

            foreach (var piece in pieces)
            {
                var legalMove = board.GetLegalMoves(piece).SingleOrDefault(move => move.Piece.GetSquare().Equals(position) && move.GetTarget().Equals(square) && move.PromotionType == promotionType);

                if (legalMove != null)
                {
                    return legalMove;
                }
            }

            throw new PgnParsingException(Raw, "unable to find a legal move to match");
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] {pieceType} from {position} to {square}";
        }
    }
}
