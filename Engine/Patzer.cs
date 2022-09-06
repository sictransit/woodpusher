using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Interfaces;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public IBoard Board { get; private set; }

        IBoard IEngine.Board => Board;

        private readonly MovementCache movementCache;

        public Patzer()
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Debug);

            movementCache = new MovementCache();

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
            var kingSquare = Board.FindKing(Board.ActiveColour);

            var validMoves = GetValidMoves(Board.ActiveColour.OpponentColour());

            return validMoves.Any(m => m.Target.Square.Equals(kingSquare));
        }

        private IEnumerable<Move> GetValidMoves(Position position)
        {
            foreach (IEnumerable<Target> vector in movementCache.GetVectors(position))
            {
                foreach (var target in vector)
                {
                    var move = new Move(position, target);

                    if (!IsValidMove(move))
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

        private bool EnPassantWithoutTarget(Move move) => move.Target.Flags.HasFlag(SpecialMove.EnPassant) && !move.Target.Square.Equals(Board.EnPassantTarget);

        private bool TookPiece(Move move) => Board.IsOccupied(move.Target.Square, move.Position.Piece.Colour.OpponentColour());

        public bool IsValidMove(Move move)
        {
            if (TakingOwnPiece(move))
            {
                return false;
            }

            if (MustTakeButCannot(move))
            {
                return false;
            }

            if (move.Position.Piece.Type == PieceType.Pawn)
            {
                if (EnPassantWithoutTarget(move))
                {
                    return false;
                }

                if (PawnCannotTakeForward(move))
                {
                    return false;
                }
            }

            if (CastleButMayNot(move.Target))
            {
                return false;
            }

            return true;
        }
    }
}