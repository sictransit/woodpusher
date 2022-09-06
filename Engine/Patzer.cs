using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer
    {
        public Board Board { get; private set; }

        private readonly MovementCache movementCache;

        public Patzer()
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Debug);

            movementCache = new MovementCache();
        }

        public void Initialize()
        {
            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
        }


        public void Initialize(string position)
        {
            Board = ForsythEdwardsNotation.Parse(position);
        }

        public IEnumerable<Move> GetValidMoves(PieceColour colour)
        {
            foreach (var position in Board.GetPositions(colour))
            {
                foreach (var move in GetValidMoves(position))
                {
                    yield return move;
                }
            }
        }

        public bool IsChecked()
        {
            var kingSquare = Board.FindKing(Board.Counters.ActiveColour);

            var validMoves = GetValidMoves(Board.Counters.ActiveColour.OpponentColour());

            return validMoves.Any(m => m.Target.Square.Equals(kingSquare));
        }

        private IEnumerable<Move> GetValidMoves(Position position)
        {
            foreach (IEnumerable<Target> vector in movementCache.GetVectors(position))
            {
                foreach (var target in vector)
                {
                    var move = new Move(position, target);

                    if (TakingOwnPiece(move))
                    {
                        break;
                    }

                    if (MustTakeButCannot(move))
                    {
                        break;
                    }

                    if (position.Piece.Type == PieceType.Pawn)
                    {
                        if (EnPassantWithoutTarget(move))
                        {
                            break;
                        }

                        if (PawnCannotTakeForward(move))
                        {
                            break;
                        }
                    }

                    if (CastleButMayNot(target))
                    {
                        break;
                    }

                    yield return move;

                    if (TookPiece(move))
                    {
                        break;
                    }
                }
            }
        }

        // TODO: Something clever with the flags, making it easy to evaluate regardless of active colour.
        private bool CastleButMayNot(Target move) => false;

        private bool TakingOwnPiece(Move move) => Board.IsOccupied(move.Target.Square, move.Position.Piece.Colour);

        private bool MustTakeButCannot(Move move) => move.Target.Flags.HasFlag(SpecialMove.MustTake) && (!Board.IsOccupied(move.Target.Square, move.Position.Piece.Colour.OpponentColour()));

        private bool PawnCannotTakeForward(Move move) => Board.IsOccupied(move.Target.Square);

        private bool EnPassantWithoutTarget(Move move) => move.Target.Flags.HasFlag(SpecialMove.EnPassant) && !move.Target.Square.Equals(Board.Counters.EnPassantTarget);

        private bool TookPiece(Move move) => Board.IsOccupied(move.Target.Square, move.Position.Piece.Colour.OpponentColour());

    }
}