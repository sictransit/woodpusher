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
        public Piece ActiveColour { get; private set; }
        public Castlings Castlings { get; private set; }
        public Square? EnPassantTarget { get; private set; }

        private readonly MovementCache movementCache;

        public Piece OppenentColour => ActiveColour.OpponentColour();

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

        public IEnumerable<Ply> GetValidPly(Piece colour)
        {
            foreach (var position in Board.GetPositions(colour))
            {
                foreach (IEnumerable<Move> vector in movementCache.GetVectors(position))
                {
                    foreach (var move in vector)
                    {
                        if (TakingOwnPiece(move, colour))
                        {
                            break;
                        }

                        if (MustTakeButCannot(move, colour))
                        {
                            break;
                        }

                        if (position.Piece.HasFlag(Piece.Pawn))
                        {
                            if (EnPassantWithoutTarget(move))
                            {
                                break;
                            }

                            if (PawnCannotTakeForward(position, move))
                            {
                                break;
                            }
                        }

                        if (CastleButMayNot(move))
                        {
                            break;
                        }


                        var ply = new Ply(position, move);

                        //Log.Debug($"valid: {ply}");

                        yield return ply;

                        if (TookPiece(move, colour.OpponentColour()))
                        {
                            break;
                        }
                    }
                }
            }
        }

        public bool IsChecked()
        {
            var kingSquare = Board.FindKing(ActiveColour);

            var opponentPlys = GetValidPly(OppenentColour);

            foreach (var ply in opponentPlys)
            {
                if (ply.Move.Square.Equals(kingSquare))
                {
                    return true;
                }
            }

            return false;

        }

        // TODO: Something clever with the flags, making it easy to evaluate regardless of active colour.
        private bool CastleButMayNot(Move move) => false;

        private bool TakingOwnPiece(Move move, Piece colour) => Board.IsOccupied(move.Square) && Board.Get(move.Square).HasFlag(colour);

        private bool MustTakeButCannot(Move move, Piece colour) => move.Flags.HasFlag(SpecialMove.MustTake) && (!Board.IsOccupied(move.Square) || Board.Get(move.Square).HasFlag(colour));

        private bool PawnCannotTakeForward(Position position, Move move) => position.Piece.HasFlag(Piece.Pawn) && Board.IsOccupied(move.Square) && !move.Flags.HasFlag(SpecialMove.MustTake);

        private bool EnPassantWithoutTarget(Move move) => move.Flags.HasFlag(SpecialMove.EnPassant) && !move.Square.Equals(EnPassantTarget);

        private bool TookPiece(Move move, Piece opponentColour) => Board.IsOccupied(move.Square) && Board.Get(move.Square).HasFlag(opponentColour);

    }
}