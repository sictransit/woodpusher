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

        public IEnumerable<Ply> GetValidPly(PieceColour colour)
        {
            foreach (var position in Board.GetPositions(colour))
            {
                foreach (var ply in GetValidMoves(position))
                {
                    yield return ply;
                }
            }
        }

        public bool IsChecked()
        {
            var kingSquare = Board.FindKing(Board.Counters.ActiveColour);

            var validPly = GetValidPly(Board.Counters.ActiveColour.OpponentColour());

            return validPly.Any(p => p.Target.Square.Equals(kingSquare));
        }

        private IEnumerable<Ply> GetValidMoves(Position position)
        {
            foreach (IEnumerable<Target> vector in movementCache.GetVectors(position))
            {
                foreach (var move in vector)
                {
                    var ply = new Ply(position, move);

                    if (TakingOwnPiece(ply))
                    {
                        break;
                    }

                    if (MustTakeButCannot(ply))
                    {
                        break;
                    }

                    if (position.Piece.Type == PieceType.Pawn)
                    {
                        if (EnPassantWithoutTarget(ply))
                        {
                            break;
                        }

                        if (PawnCannotTakeForward(ply))
                        {
                            break;
                        }
                    }

                    if (CastleButMayNot(move))
                    {
                        break;
                    }

                    //Log.Debug($"valid: {ply}");

                    yield return ply;

                    if (TookPiece(ply))
                    {
                        break;
                    }
                }
            }
        }

        // TODO: Something clever with the flags, making it easy to evaluate regardless of active colour.
        private bool CastleButMayNot(Target move) => false;

        private bool TakingOwnPiece(Ply ply) => Board.IsOccupied(ply.Target.Square, ply.Position.Piece.Colour);

        private bool MustTakeButCannot(Ply ply) => ply.Target.Flags.HasFlag(SpecialMove.MustTake) && (!Board.IsOccupied(ply.Target.Square, ply.Position.Piece.Colour.OpponentColour()));

        private bool PawnCannotTakeForward(Ply ply) => Board.IsOccupied(ply.Target.Square);

        private bool EnPassantWithoutTarget(Ply ply) => ply.Target.Flags.HasFlag(SpecialMove.EnPassant) && !ply.Target.Square.Equals(Board.Counters.EnPassantTarget);

        private bool TookPiece(Ply ply) => Board.IsOccupied(ply.Target.Square, ply.Position.Piece.Colour.OpponentColour());

    }
}