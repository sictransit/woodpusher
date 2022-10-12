using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class Attacks
    {
        private readonly Dictionary<Pieces, ThreatMask> threatMasks = new();

        public Attacks()
        {
            Initialize();
        }

        public ThreatMask GetThreatMask(Pieces piece) => threatMasks[piece];

        private void Initialize()
        {
            foreach (var color in new[] { Pieces.White, Pieces.Black })
            {
                var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

                foreach (var square in squares)
                {
                    var queenMask = QueenMovement.GetTargetVectors((Pieces.Queen| color).SetSquare( square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    var bishopMask = BishopMovement.GetTargetVectors((Pieces.Queen | color).SetSquare(square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    var knightMask = KnightMovement.GetTargetVectors((Pieces.Queen | color).SetSquare(square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    var rookMask = RookMovement.GetTargetVectors((Pieces.Queen | color).SetSquare(square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                    var kingMask = KingMovement.GetTargetVectors((Pieces.Queen | color.OpponentColor()).SetSquare(square)).SelectMany(v => v).Where(v => !v.Flags.HasFlag(SpecialMove.CastleQueen) && !v.Flags.HasFlag(SpecialMove.CastleKing)).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());

                    var pawnMask = 0ul;

                    switch (color)
                    {
                        case Pieces.White when square.Rank < 6:
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
                        case Pieces.Black when square.Rank > 1:
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

                    threatMasks.Add(color.SetSquare( square), new ThreatMask(pawnMask, rookMask, knightMask, bishopMask, queenMask, kingMask));
                }
            }
        }
    }
}
