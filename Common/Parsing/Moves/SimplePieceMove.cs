using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class SimplePieceMove : PgnMove
    {
        private readonly Piece pieceType;
        private readonly Square square;

        public SimplePieceMove(Piece pieceType, Square square)
        {
            this.pieceType = pieceType;
            this.square = square;
        }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var pieces = board.GetPieces(board.ActiveColor, pieceType);

            foreach (var piece in pieces)
            {
                var move = board.GetLegalMoves(piece).SingleOrDefault(m => m.GetTarget().Equals(square));

                if (move != null)
                {
                    return move;
                }
            }

            throw new PgnParsingException(Raw, "unable to a legal move to match");
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] {pieceType} to {square}";
        }
    }
}
