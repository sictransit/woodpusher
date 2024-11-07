using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class Moves
    {
        private readonly Dictionary<Piece, Move[][]> vectors = new();
        private readonly Dictionary<ulong, ulong> travelMasks = new();        

        public Moves()
        {
            InitializeVectors();

            InitializeTravelMasks();
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

        public ulong GetTravelMask(ulong current, ulong target) => travelMasks[current | target];

        private void InitializeVectors()
        {
            PieceExtensions.AllPieces.ToList().ForEach(p =>
            {
                SquareExtensions.AllSquares.ToList().ForEach(s =>
                {
                    var piece = p.SetSquare(s);

                    vectors.Add(piece, CreateVectors(piece).Select(v => v.ToArray()).ToArray());
                });
            });
        }

        public Move[][] GetVectors(Piece piece) => vectors[piece];

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
