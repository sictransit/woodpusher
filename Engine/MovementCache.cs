using Serilog;
using SicTransit.Woodpusher.Engine.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Engine
{
    internal class MovementCache
    {
        private readonly Dictionary<Position, List<Move[]>> vectors = new Dictionary<Position, List<Move[]>>();
        private readonly Dictionary<Square, ulong> checks = new();

        public MovementCache()
        {
            InitializeVectors();
            InitializeChecks();
        }

        private void InitializeVectors()
        {
            var pieces = new[] { Piece.White, Piece.Black }.Select(c => new[] { Piece.Pawn, Piece.Rook, Piece.Knight, Piece.Bishop, Piece.Queen, Piece.King }.Select(t => c | t)).SelectMany(x => x).ToList();
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            pieces.ForEach(piece =>
            {
                squares.ForEach(square =>
                {
                    var position = new Position(piece, square);

                    vectors.Add(position, CreateVectors(position).Select(v => v.ToArray()).ToList());

                    Log.Debug($"Calculated vectors: {position}");
                });
            });
        }

        private void InitializeChecks()
        {
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            squares.ForEach(square =>
            {
                ulong mask = 0;

                var moves = GetVectors(new Position(Piece.Queen | Piece.White, square)).Concat(GetVectors(new Position(Piece.Knight | Piece.White, square))).SelectMany(v => v);

                foreach (var move in moves)
                {
                    mask |= move.Square.ToMask();
                }

                checks.Add(square, mask);
            });
        }


        public List<Move[]> GetVectors(Position position)
        {
            return vectors[position];
        }

        public ulong GetCheckMask(Square square)
        {
            return checks[square];
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
