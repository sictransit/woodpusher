using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class Attacks
    {
        private readonly Dictionary<PieceColor, Dictionary<ulong, ThreatMask>> threatMasks = new();

        public Attacks()
        {
            Initialize();
        }

        public ThreatMask GetThreatMask(PieceColor pieceColor, ulong current) => threatMasks[pieceColor][current];

        private void Initialize()
        {
            foreach (var color in new[] { PieceColor.White, PieceColor.Black })
            {
                threatMasks.Add(color, new Dictionary<ulong, ThreatMask>());

                var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

                foreach (var square in squares)
                {
                    var queenMask = QueenMovement.GetTargetVectors(new Position(new Piece(PieceType.Queen, color), square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    var bishopMask = BishopMovement.GetTargetVectors(new Position(new Piece(PieceType.Bishop, color), square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    var knightMask = KnightMovement.GetTargetVectors(new Position(new Piece(PieceType.Knight, color), square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    var rookMask = RookMovement.GetTargetVectors(new Position(new Piece(PieceType.Rook, color), square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    var kingMask = KingMovement.GetTargetVectors(new Position(new Piece(PieceType.King, color.OpponentColor()), square)).SelectMany(v => v).Where(v => !v.Flags.HasFlag(SpecialMove.CastleQueen) && !v.Flags.HasFlag(SpecialMove.CastleKing)).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    
                    var pawnMask = 0ul;

                    switch (color)
                    {
                        case PieceColor.White when square.Rank < 6:
                            {
                                if (Square.TryCreate(square.File - 1, square.Rank + 1, out var upLeft))
                                {
                                    pawnMask |= upLeft.ToMask();
                                }
                                if (Square.TryCreate(square.File + 1, square.Rank + 1, out var upRight))
                                {
                                    pawnMask |= upRight.ToMask();
                                }

                                break;
                            }
                        case PieceColor.Black when square.Rank > 1:
                            {
                                if (Square.TryCreate(square.File - 1, square.Rank - 1, out var downLeft))
                                {
                                    pawnMask |= downLeft.ToMask();
                                }
                                if (Square.TryCreate(square.File + 1, square.Rank - 1, out var downRight))
                                {
                                    pawnMask |= downRight.ToMask();
                                }

                                break;
                            }
                    }

                    threatMasks[color].Add(square.ToMask(), new ThreatMask(pawnMask, rookMask, knightMask, bishopMask, queenMask, kingMask));
                }
            }
        }
    }
}
