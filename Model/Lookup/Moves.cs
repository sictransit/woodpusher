using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Model.Lookup
{
    public class Moves
    {
        private readonly Dictionary<Piece, Dictionary<Square, List<Target[]>>> vectors = new();
        private readonly Dictionary<Square, Dictionary<Square, ulong>> travelMasks = new();

        public Moves()
        {
            InitializeVectors();

            InitializeTravelMasks();
        }

        private void InitializeTravelMasks()
        {
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToArray();

            foreach (var square in squares)
            {
                travelMasks.Add(square, new Dictionary<Square, ulong>());

                foreach (var target in squares)
                {
                    travelMasks[square].Add(target, square.ToTravelMask(target));
                }
            }
        }

        public ulong GetTravelMask(Square square, Square target) => travelMasks[square][target];

        private void InitializeVectors()
        {
            var pieces = new[] { PieceColour.White, PieceColour.Black }.Select(c => new[] { PieceType.Pawn, PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King }.Select(t => new Piece(t, c))).SelectMany(x => x).ToList();
            var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

            pieces.ForEach(piece =>
            {
                vectors.Add(piece, new Dictionary<Square, List<Target[]>>());

                squares.ForEach(square =>
                {
                    var position = new Position(piece, square);

                    vectors[piece].Add(square, CreateVectors(position).Select(v => v.ToArray()).ToList());

                    //Log.Debug($"Calculated vectors: {position}");
                });
            });
        }

        public List<Target[]> GetVectors(Position position)
        {
            return vectors[position.Piece][position.Square];
        }

        private static IEnumerable<IEnumerable<Target>> CreateVectors(Position position) => position.Piece.Type switch
        {
            PieceType.Pawn => PawnMovement.GetTargetVectors(position.Square, position.Piece.Colour),
            PieceType.Rook => RookMovement.GetTargetVectors(position.Square),
            PieceType.Knight => KnightMovement.GetTargetVectors(position.Square),
            PieceType.Bishop => BishopMovement.GetTargetVectors(position.Square),
            PieceType.Queen => QueenMovement.GetTargetVectors(position.Square),
            PieceType.King => KingMovement.GetTargetVectors(position.Square, position.Piece.Colour),
            _ => throw new ArgumentOutOfRangeException(nameof(position)),
        };
    }
}
