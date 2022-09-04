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
            var pieces = new[] { PieceColour.White, PieceColour.Black }.Select(c => new[] { PieceType.Pawn, PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King }.Select(t => new Piece(t, c))).SelectMany(x => x).ToList();
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

            var whiteQueen = new Piece(PieceType.Queen, PieceColour.White);
            var whiteKnight = new Piece(PieceType.Knight, PieceColour.White);

            squares.ForEach(square =>
            {
                ulong mask = 0;

                var queenVectors = GetVectors(new Position(whiteQueen, square));
                var knightVectors = GetVectors(new Position(whiteKnight, square));
                var moves = queenVectors.Concat(knightVectors).SelectMany(v => v);

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

        private static IEnumerable<IEnumerable<Move>> CreateVectors(Position position) => position.Piece.Type switch
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
