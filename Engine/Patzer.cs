using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer : IEngine
    {
        public Board Board { get; private set; }

        private readonly Stack<Board> game;

        private readonly MovementCache movementCache;

        public Patzer()
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Debug);

            movementCache = new MovementCache();
            game = new Stack<Board>();

            Board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);
        }

        public void Initialize(string position)
        {
            game.Clear();

            Board = ForsythEdwardsNotation.Parse(position);
        }

        public void Play(Move move)
        {
            game.Push(Board);

            Board = Board.Play(move);
        }

        public IEnumerable<Move> GetValidMoves(PieceColour colour, ulong mask = ulong.MaxValue)
        {
            foreach (var position in Board.GetPositions(colour, mask))
            {
                foreach (var move in GetValidMoves(position))
                {
                    yield return move;
                }
            }
        }

        public bool IsChecked(Square kingSquare)
        {
            var checkMask = movementCache.GetCheckMask(kingSquare);

            var validMoves = GetValidMoves(Board.ActiveColour.OpponentColour(), checkMask).ToArray();

            return validMoves.Any(m => m.Target.Square.Equals(kingSquare));
        }

        private IEnumerable<Move> GetValidMoves(Position position)
        {
            foreach (IEnumerable<Target> vector in movementCache.GetVectors(position))
            {
                foreach (var target in vector)
                {
                    var move = new Move(position, target);

                    if (!Validate(move))
                    {
                        break;
                    }

                    if (move.Position.Piece.Type == PieceType.King)
                    {
                        if (move.Target.Flags.HasFlag(SpecialMove.CastleQueen) || move.Target.Flags.HasFlag(SpecialMove.CastleKing))
                        {
                            if (CastleFromOrIntoCheck(move))
                            {
                                break;
                            }
                        }
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
        private bool CastleFromOrIntoCheck(Move move) => IsChecked(move.Position.Square) || IsChecked(move.Target.ReferenceSquare!.Value) || IsChecked(move.Target.Square);

        private bool TakingOwnPiece(Move move) => Board.IsOccupied(move.Target.Square, move.Position.Piece.Colour);

        private bool MustTakeButCannot(Move move) => move.Target.Flags.HasFlag(SpecialMove.MustTake) && (!Board.IsOccupied(move.Target.Square, move.Position.Piece.Colour.OpponentColour()));

        private bool PawnCannotTakeForward(Move move) => Board.IsOccupied(move.Target.Square);

        private bool EnPassantWithoutTarget(Move move) => move.Target.Flags.HasFlag(SpecialMove.EnPassant) && !move.Target.Square.Equals(Board.Counters.EnPassantTarget);

        private bool TookPiece(Move move) => Board.IsOccupied(move.Target.Square, move.Position.Piece.Colour.OpponentColour());

        private bool Validate(Move move)
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

            return true;
        }

        public Move GetMove(Position position, Square targetSquare)
        {
            var validMoves = GetValidMoves(position);

            Move move = validMoves.SingleOrDefault(m => m.Target.Square.Equals(targetSquare));

            return move;
        }

        public IEnumerable<Move> GetMoves(Position position) => GetValidMoves(position);
    }
}