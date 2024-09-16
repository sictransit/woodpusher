using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public abstract class CastlingMove : PgnMove
    {
        protected abstract Piece CastlingPiece { get; }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var kingPiece = board.GetPieces(board.ActiveColor, Piece.King).Single();

            var moves = board.GetLegalMoves(kingPiece).Select(l => l.Move);

            foreach (var move in moves)
            {
                if (move.Flags.HasFlag(SpecialMove.CastleKing) && CastlingPiece.Is(Piece.King))
                {
                    return move;
                }

                if (move.Flags.HasFlag(SpecialMove.CastleQueen) && CastlingPiece.Is(Piece.Queen))
                {
                    return move;
                }
            }


            throw new PgnParsingException(Raw, "unable to find a legal move to match");
        }
    }
}
