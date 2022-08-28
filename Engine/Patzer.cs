using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer
    {
        public Board Board { get; private set; }
        public Piece ActiveColour { get; private set; }
        public Castlings Castlings { get; private set; }

        public Patzer()
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Debug);
        }


        public void Initialize(string position)
        {
            var fen = FEN.Parse(position);

            Board = fen.Board;
            ActiveColour = fen.ActiveColour;
            Castlings = fen.Castlings;
        }

        public IEnumerable<Ply> GetValidPly()
        {
            foreach (var position in Board.GetPositions(ActiveColour))
            {
                foreach (IEnumerable<Move> vector in GetVectors(position))
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

                        yield return new Ply(position, move);
                    }
                }
            }
        }

        private bool TakingOwnPiece(Move move) => Board.IsOccupied(move.Square) && Board.Get(move.Square).HasFlag(ActiveColour);

        private bool MustTakeButCannot(Move move) => move.Flags.HasFlag(MovementFlags.MustTake) && (!Board.IsOccupied(move.Square) || Board.Get(move.Square).HasFlag(ActiveColour));

        private static IEnumerable<IEnumerable<Move>> GetVectors(Position position)
        {
            var colour = position.Piece.HasFlag(Piece.White) ? Piece.White : Piece.Black;

            switch ((int)position.Piece & Constants.PIECETYPE)
            {
                case Constants.PAWN:
                    return PawnMovement.GetTargetVectors(position.Square, colour);
                case Constants.ROOK:
                    return RookMovement.GetTargetVectors(position.Square);
                case Constants.KNIGHT:
                    return KnightMovement.GetTargetVectors(position.Square);
                case Constants.BISHOP:
                    return BishopMovement.GetTargetVectors(position.Square);
                case Constants.QUEEN:
                    return QueenMovement.GetTargetVectors(position.Square);
                case Constants.KING:
                    return KingMovement.GetTargetVectors(position.Square, colour);
                default:
                    throw new ArgumentOutOfRangeException(nameof(position));
            }
        }
    }
}