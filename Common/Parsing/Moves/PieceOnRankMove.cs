using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class PieceOnRankMove : PgnMove
    {
        private readonly Piece pieceType;
        private readonly int rank;
        private readonly Square square;
        private readonly Piece promotionType;

        public PieceOnRankMove(Piece pieceType, int rank, Square square, Piece promotionType)
        {
            this.pieceType = pieceType;
            this.rank = rank;
            this.square = square;
            this.promotionType = promotionType;
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] {pieceType} from rank {rank} to {square}";
        }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var pieces = board.GetPieces(board.ActiveColor, pieceType);

            foreach (var piece in pieces.Where(p => p.GetSquare().Rank == rank))
            {
                var legalMove = engine.Board.GetLegalMoves(piece).SingleOrDefault(l => l.GetTarget().Equals(square) && l.PromotionType == promotionType);

                if (legalMove != null)
                {
                    return legalMove;
                }
            }

            throw new PgnParsingException(Raw, "unable to a legal move to match");
        }
    }
}
