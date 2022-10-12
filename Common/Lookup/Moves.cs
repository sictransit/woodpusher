using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class Moves
    {
        private readonly Dictionary<Pieces, IReadOnlyCollection<Move[]>> vectors = new();
        private readonly Dictionary<ulong, ulong> travelMasks = new();
        private readonly Dictionary<Pieces, ulong> passedPawnMasks = new();

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
                foreach (var target in squares)
                {
                    var mask = square | target;
                    if (!travelMasks.ContainsKey(mask))
                    {
                        travelMasks.Add(mask, square.ToSquare().ToTravelPath(target.ToSquare()).ToMask());
                    }
                }
            }
        }

        private void InitializePassedPawnMasks()
        {
            var pieces = new[] { Pieces.White, Pieces.Black }.Select(c => Pieces.Pawn | c);
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(1, 6).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            var positions = pieces.Select(p => squares.Select(s => p.SetSquare(s))).SelectMany(p => p);

            foreach (var position in positions)
            {
                var mask = 0ul;
                var minRank = position.Is(Pieces.White) ? position.GetSquare().Rank + 1 : 1;
                var maxRank = position.Is(Pieces.White) ? 6 : position.GetSquare().Rank - 1;

                foreach (var dFile in new[] { -1, 0, 1 })
                {
                    for (var rank = minRank; rank <= maxRank; rank++)
                    {
                        if (Square.TryCreate(position.GetSquare().File + dFile, rank, out var square))
                        {
                            mask |= square.ToMask();
                        }
                    }
                }

                passedPawnMasks.Add(position, mask);
            }
        }

        public ulong GetTravelMask(ulong current, ulong target) => travelMasks[current | target];

        public ulong GetPassedPawnMask(Pieces position) => passedPawnMasks[position];

        private void InitializeVectors()
        {
            var pieces = new[] { Pieces.White, Pieces.Black }.Select(c => new[] { Pieces.Pawn, Pieces.Rook, Pieces.Knight, Pieces.Bishop, Pieces.Queen, Pieces.King }.Select(t => t|c)).SelectMany(x => x).ToList();
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            pieces.ForEach(piece =>
            {
                squares.ForEach(square =>
                {
                    var position = piece.SetSquare( square);

                    vectors.Add(position, CreateVectors(position).Select(v => v.ToArray()).ToArray());
                });
            });
        }

        public IReadOnlyCollection<Move[]> GetVectors(Pieces position)
        {
            return vectors[position];
        }

        private static IEnumerable<IEnumerable<Move>> CreateVectors(Pieces position) 
        {
            if (position.Is(Pieces.Pawn))
            {
                return PawnMovement.GetTargetVectors(position);
            }

            if (position.Is(Pieces.Rook))
            {
                return RookMovement.GetTargetVectors(position);
            }

            if (position.Is(Pieces.Knight))
            {
                return KnightMovement.GetTargetVectors(position);
            }

            if (position.Is(Pieces.Bishop))
            {
                return BishopMovement.GetTargetVectors(position);
            }

            if (position.Is(Pieces.Queen))
            {
                return QueenMovement.GetTargetVectors(position);
            }

            if (position.Is(Pieces.King))
            {
                return KingMovement.GetTargetVectors(position);
            }

            throw new ArgumentOutOfRangeException(nameof(position));
        }
    }
}
