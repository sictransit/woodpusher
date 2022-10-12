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
            var pieceTypes = new[] { Pieces.White, Pieces.None }.Select(c => Pieces.Pawn | c);
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(1, 6).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            var pieces = pieceTypes.Select(p => squares.Select(s => p.SetSquare(s))).SelectMany(p => p);

            foreach (var piece in pieces)
            {
                var mask = 0ul;
                var minRank = piece.Is(Pieces.White) ? piece.GetSquare().Rank + 1 : 1;
                var maxRank = piece.Is(Pieces.White) ? 6 : piece.GetSquare().Rank - 1;

                foreach (var dFile in new[] { -1, 0, 1 })
                {
                    for (var rank = minRank; rank <= maxRank; rank++)
                    {
                        if (Square.TryCreate(piece.GetSquare().File + dFile, rank, out var square))
                        {
                            mask |= square.ToMask();
                        }
                    }
                }

                passedPawnMasks.Add(piece, mask);
            }
        }

        public ulong GetTravelMask(ulong current, ulong target) => travelMasks[current | target];

        public ulong GetPassedPawnMask(Pieces piece) => passedPawnMasks[piece];

        private void InitializeVectors()
        {
            var pieces = new[] { Pieces.White, Pieces.None }.Select(c => new[] { Pieces.Pawn, Pieces.Rook, Pieces.Knight, Pieces.Bishop, Pieces.Queen, Pieces.King }.Select(t => t | c)).SelectMany(x => x).ToList();
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            pieces.ForEach(p =>
            {
                squares.ForEach(s =>
                {
                    var piece = p.SetSquare(s);

                    vectors.Add(piece, CreateVectors(piece).Select(v => v.ToArray()).ToArray());
                });
            });
        }

        public IReadOnlyCollection<Move[]> GetVectors(Pieces piece)
        {
            return vectors[piece];
        }

        private static IEnumerable<IEnumerable<Move>> CreateVectors(Pieces piece)
        {
            if (piece.Is(Pieces.Pawn))
            {
                return PawnMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Pieces.Rook))
            {
                return RookMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Pieces.Knight))
            {
                return KnightMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Pieces.Bishop))
            {
                return BishopMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Pieces.Queen))
            {
                return QueenMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Pieces.King))
            {
                return KingMovement.GetTargetVectors(piece);
            }

            throw new ArgumentOutOfRangeException(nameof(piece));
        }
    }
}
