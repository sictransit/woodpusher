using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class Moves
    {
        private readonly Dictionary<Position, IReadOnlyCollection<Move[]>> vectors = new();
        private readonly Dictionary<ulong, Dictionary<ulong, ulong>> travelMasks = new();
        private readonly Dictionary<Position, ulong> passedPawnMasks = new();

        public Moves()
        {
            InitializeVectors();

            InitializeTravelMasks();

            InitializePassedPawnMasks();
        }

        private void InitializeTravelMasks()
        {
            var squares = Enumerable.Range(0, 64).Select(shift => 1ul << shift);

            foreach (var square in squares)
            {
                travelMasks.Add(square, new Dictionary<ulong, ulong>());

                foreach (var target in squares)
                {
                    travelMasks[square].Add(target, square.ToSquare().ToTravelMask(target.ToSquare()));
                }
            }
        }

        private void InitializePassedPawnMasks()
        {
            var pieces = new[] { PieceColor.White, PieceColor.Black }.Select(c => new Piece(PieceType.Pawn, c));
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(1, 6).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            var positions = pieces.Select(p => squares.Select(s => new Position(p, s))).SelectMany(p => p);

            foreach (var position in positions)
            {
                var mask = 0ul;
                var sign = position.Piece.Color == PieceColor.White ? 1 : -1;
                var minRank = position.Piece.Color == PieceColor.White ? position.Square.Rank + 1 : 1;
                var maxRank = position.Piece.Color == PieceColor.White ? 6 : position.Square.Rank - 1;

                foreach (var dFile in new[] { -1, 0, 1 })
                {
                    for (int rank = minRank; rank <= maxRank; rank++)
                    {
                        if (Square.TryCreate(position.Square.File + dFile, rank, out var square))
                        {
                            mask |= square.ToMask();
                        }
                    }
                }

                passedPawnMasks.Add(position, mask);
            }
        }

        public ulong GetTravelMask(ulong current, ulong target) => travelMasks[current][target];

        public ulong GetPassedPawnMask(Position position) => passedPawnMasks[position];

        private void InitializeVectors()
        {
            var pieces = new[] { PieceColor.White, PieceColor.Black }.Select(c => new[] { PieceType.Pawn, PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King }.Select(t => new Piece(t, c))).SelectMany(x => x).ToList();
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            pieces.ForEach(piece =>
            {
                squares.ForEach(square =>
                {
                    var position = new Position(piece, square);

                    vectors.Add(position, CreateVectors(position).Select(v => v.ToArray()).ToArray());
                });
            });
        }

        public IReadOnlyCollection<Move[]> GetVectors(Position position)
        {
            return vectors[position];
        }

        private static IEnumerable<IEnumerable<Move>> CreateVectors(Position position) => position.Piece.Type switch
        {
            PieceType.Pawn => PawnMovement.GetTargetVectors(position),
            PieceType.Rook => RookMovement.GetTargetVectors(position),
            PieceType.Knight => KnightMovement.GetTargetVectors(position),
            PieceType.Bishop => BishopMovement.GetTargetVectors(position),
            PieceType.Queen => QueenMovement.GetTargetVectors(position),
            PieceType.King => KingMovement.GetTargetVectors(position),
            _ => throw new ArgumentOutOfRangeException(nameof(position)),
        };
    }
}
