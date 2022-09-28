using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Model.Lookup
{
    public class Moves
    {
        private readonly Dictionary<Piece, Dictionary<Square, List<Move[]>>> vectors = new();
        private readonly Dictionary<ulong, Dictionary<ulong, ulong>> travelMasks = new();

        public Moves()
        {
            InitializeVectors();

            InitializeTravelMasks();
        }

        private void InitializeTravelMasks()
        {
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r).ToMask())).SelectMany(x => x).ToArray();

            foreach (var square in squares)
            {
                travelMasks.Add(square, new Dictionary<ulong, ulong>());

                foreach (var target in squares)
                {
                    travelMasks[square].Add(target, square.ToSquare().ToTravelMask(target.ToSquare()));
                }
            }
        }

        public ulong GetTravelMask(ulong current, ulong target) => travelMasks[current][target];

        private void InitializeVectors()
        {
            var pieces = new[] { PieceColor.White, PieceColor.Black }.Select(c => new[] { PieceType.Pawn, PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King }.Select(t => new Piece(t, c))).SelectMany(x => x).ToList();
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            pieces.ForEach(piece =>
            {
                vectors.Add(piece, new Dictionary<Square, List<Move[]>>());

                squares.ForEach(square =>
                {
                    var position = new Position(piece, square);

                    vectors[piece].Add(square, CreateVectors(position).Select(v => v.ToArray()).ToList());

                    //Log.Debug($"Calculated vectors: {position}");
                });
            });
        }

        public List<Move[]> GetVectors(Position position)
        {
            return vectors[position.Piece][position.Square];
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
