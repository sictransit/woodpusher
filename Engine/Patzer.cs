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
        public PieceColour ActiveColour { get; private set; }
        public Castlings Castlings { get; private set; }
        public Square? EnPassantTarget { get; private set; }

        private readonly MovementCache movementCache;

        public Patzer()
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Debug);

            movementCache = new MovementCache();
        }


        public void Initialize(string position)
        {
            var fen = ForsythEdwardsNotation.Parse(position);

            Board = fen.Board;
            ActiveColour = fen.ActiveColour;
            Castlings = fen.Castlings;
            EnPassantTarget = fen.EnPassantTarget;
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
            var kingSquare = Board.FindKing(ActiveColour);

            foreach (var ply in GetValidPly(ActiveColour.OpponentColour()))
            {
                if (ply.Move.Square.Equals(kingSquare))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Ply> GetValidMoves(Position position)
        {
            foreach (IEnumerable<Move> vector in movementCache.GetVectors(position))
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
        private bool CastleButMayNot(Move move) => false;

        private bool TakingOwnPiece(Ply ply) => Board.IsOccupied(ply.Move.Square, ply.Position.Piece.Colour);

        private bool MustTakeButCannot(Ply ply) => ply.Move.Flags.HasFlag(SpecialMove.MustTake) && (!Board.IsOccupied(ply.Move.Square, ply.Position.Piece.Colour.OpponentColour()));

        private bool PawnCannotTakeForward(Ply ply) => Board.IsOccupied(ply.Move.Square);

        private bool EnPassantWithoutTarget(Ply ply) => ply.Move.Flags.HasFlag(SpecialMove.EnPassant) && !ply.Move.Square.Equals(EnPassantTarget);

        private bool TookPiece(Ply ply) => Board.IsOccupied(ply.Move.Square, ply.Position.Piece.Colour.OpponentColour());

    }
}