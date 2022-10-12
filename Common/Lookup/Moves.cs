using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class Moves
    {
        private readonly Dictionary<Piece, IReadOnlyCollection<Move[]>> vectors = new();
        private readonly Dictionary<ulong, ulong> travelMasks = new();
        private readonly Dictionary<Piece, ulong> passedPawnMasks = new();

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
            var pieceTypes = new[] { Piece.White, Piece.None }.Select(c => Piece.Pawn | c);
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(1, 6).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            var pieces = pieceTypes.Select(p => squares.Select(s => p.SetSquare(s))).SelectMany(p => p);

            foreach (var piece in pieces)
            {
                var mask = 0ul;
                var minRank = piece.Is(Piece.White) ? piece.GetSquare().Rank + 1 : 1;
                var maxRank = piece.Is(Piece.White) ? 6 : piece.GetSquare().Rank - 1;

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

        public ulong GetPassedPawnMask(Piece piece) => passedPawnMasks[piece];

        private void InitializeVectors()
        {
            var pieces = new[] { Piece.White, Piece.None }.Select(c => new[] { Piece.Pawn, Piece.Rook, Piece.Knight, Piece.Bishop, Piece.Queen, Piece.King }.Select(t => t | c)).SelectMany(x => x).ToList();
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

        public IReadOnlyCollection<Move[]> GetVectors(Piece piece)
        {
            return vectors[piece];
        }

        private static IEnumerable<IEnumerable<Move>> CreateVectors(Piece piece)
        {
            if (piece.Is(Piece.Pawn))
            {
                return PawnMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Piece.Rook))
            {
                return RookMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Piece.Knight))
            {
                return KnightMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Piece.Bishop))
            {
                return BishopMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Piece.Queen))
            {
                return QueenMovement.GetTargetVectors(piece);
            }

            if (piece.Is(Piece.King))
            {
                return KingMovement.GetTargetVectors(piece);
            }

            throw new ArgumentOutOfRangeException(nameof(piece));
        }
    }
}
