using Serilog;
using SicTransit.Woodpusher.Engine.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine
{
    internal class MovementCache
    {
        private readonly Dictionary<Position, List<Move[]>> vectorCache = new Dictionary<Position, List<Move[]>>();

        public MovementCache()
        {
            Initialize();
        }

        private void Initialize()
        {
            var pieces = new[] { Piece.White, Piece.Black }.Select(c => new[] { Piece.Pawn, Piece.Rook, Piece.Knight, Piece.Bishop, Piece.Queen, Piece.King }.Select(t => c | t)).SelectMany(x => x).ToList();
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            pieces.ForEach(piece =>
            {
                squares.ForEach(square =>
                {
                    var position = new Position(piece, square);

                    vectorCache.Add(position, CreateVectors(position).Select(v => v.ToArray()).ToList());

                    Log.Debug($"Calculated vectors: {position}");
                });
            });
        }

        public List<Move[]> GetVectors(Position position)
        {
            return vectorCache[position];
        }

        private static IEnumerable<IEnumerable<Move>> CreateVectors(Position position)
        {
            var colour = position.Piece.HasFlag(Piece.White) ? Piece.White : Piece.Black;

            switch ((int)position.Piece & Constants.PieceTypeMask)
            {
                case Constants.Pawn:
                    return PawnMovement.GetTargetVectors(position.Square, colour);
                case Constants.Rook:
                    return RookMovement.GetTargetVectors(position.Square);
                case Constants.Knight:
                    return KnightMovement.GetTargetVectors(position.Square);
                case Constants.Bishop:
                    return BishopMovement.GetTargetVectors(position.Square);
                case Constants.Queen:
                    return QueenMovement.GetTargetVectors(position.Square);
                case Constants.King:
                    return KingMovement.GetTargetVectors(position.Square, colour);
                default:
                    throw new ArgumentOutOfRangeException(nameof(position));
            }
        }

    }
}
