using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer
    {
        public Board Board { get; private set; }
        public Piece ActiveColour { get; private set; }
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
            var fen = FEN.Parse(position);

            Board = fen.Board;
            ActiveColour = fen.ActiveColour;
            Castlings = fen.Castlings;
            EnPassantTarget = fen.EnPassantTarget;
        }

        public IEnumerable<Ply> GetValidPly()
        {
            foreach (var position in Board.GetPositions(ActiveColour))
            {
                foreach (IEnumerable<Move> vector in movementCache.GetVectors(position))
                {
                    foreach (var move in vector)
                    {
                        if (TakingOwnPiece(move))
                        {
                            break;
                        }

                        if (MustTakeButCannot(move))
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

                        Log.Debug($"valid: {ply}");

                        yield return ply;

                        if (TookPiece(move))
                        {
                            break;
                        }
                    }
                }
            }
        }

        // TODO: Something clever with the flags, making it easy to evaluate regardless of active colour.
        private bool CastleButMayNot(Move move) => false;

        private bool TakingOwnPiece(Move move) => Board.IsOccupied(move.Square) && Board.Get(move.Square).HasFlag(ActiveColour);

        private bool MustTakeButCannot(Move move) => move.Flags.HasFlag(SpecialMove.MustTake) && (!Board.IsOccupied(move.Square) || Board.Get(move.Square).HasFlag(ActiveColour));

        private bool PawnCannotTakeForward(Position position, Move move) => position.Piece.HasFlag(Piece.Pawn) && Board.IsOccupied(move.Square) && !move.Flags.HasFlag(SpecialMove.MustTake);

        private bool EnPassantWithoutTarget(Move move) => move.Flags.HasFlag(SpecialMove.EnPassant) && !move.Square.Equals(EnPassantTarget);

        private bool TookPiece(Move move) => Board.IsOccupied(move.Square) && !Board.Get(move.Square).HasFlag(ActiveColour);

    }
}