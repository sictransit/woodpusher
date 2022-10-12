using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
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

            var kingPiece = board.GetPieces(board.ActiveColor, Pieces.King).Single();

            var moves = board.GetLegalMoves(kingPiece);

            foreach (var move in moves)
            {
                if (move.Flags.HasFlag(SpecialMove.CastleKing) && Castling == Castlings.Kingside)
                {
                    return move;
                }

                if (move.Flags.HasFlag(SpecialMove.CastleQueen) && Castling == Castlings.Queenside)
                {
                    return move;
                }
            }


            throw new PgnParsingException(Raw, "unable to find a legal move to match");
        }
    }
}
