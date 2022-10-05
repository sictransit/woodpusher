using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class PieceOnRankMove : PgnMove
    {
        private readonly PieceType pieceType;
        private readonly int rank;
        private readonly Square square;
        private readonly PieceType promotionType;

        public PieceOnRankMove(string raw, PieceType pieceType, int rank, Square square, PieceType promotionType) : base(raw)
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

            var positions = board.GetPositions(board.ActiveColor, pieceType);

            foreach (var position in positions.Where(p => p.Square.Rank == rank))
            {
                var move = engine.Board.GetLegalMoves(position).SingleOrDefault(m => m.GetTarget().Equals(square) && m.PromotionType == promotionType);

                if (move != null)
                {
                    return move;
                }
            }

            throw new PgnParsingException(Raw, "unable to a legal move to match");
        }
    }
}
