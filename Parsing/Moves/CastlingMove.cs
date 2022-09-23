using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Parsing.Exceptions;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public abstract class CastlingMove : PgnMove
    {
        protected CastlingMove(string raw) : base(raw)
        {
        }

        protected abstract Castlings Castling { get; }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var kingPosition = board.GetPositions(board.ActiveColor, PieceType.King).Single();

            var moves = board.GetLegalMoves(kingPosition);

            foreach (var move in moves)
            {
                if (move.Target.Flags.HasFlag(SpecialMove.CastleKing) && this.Castling == Castlings.Kingside)
                {
                    return move;
                }

                if (move.Target.Flags.HasFlag(SpecialMove.CastleQueen) && this.Castling == Castlings.Queenside)
                {
                    return move;
                }
            }


            throw new PgnParsingException(Raw, "unable to find a legal move to match");
        }
    }
}
