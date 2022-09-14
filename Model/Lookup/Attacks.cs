using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Model.Lookup
{
    public class Attacks
    {
        private readonly Dictionary<PieceColour, Dictionary<Square, ThreatMask>> threatMasks = new();

        public Attacks()
        {
            Initialize();
        }

        public ThreatMask GetThreatMask(PieceColour pieceColour, Square square) => threatMasks[pieceColour][square];

        private void Initialize()
        {
            foreach (var colour in new[] { PieceColour.White, PieceColour.Black })
            {
                threatMasks.Add(colour, new Dictionary<Square, ThreatMask>());

                var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

                foreach (var square in squares)
                {
                    var queenMask = QueenMovement.GetTargetVectors(square).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.Square.ToMask());
                    var bishopMask = BishopMovement.GetTargetVectors(square).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.Square.ToMask());
                    var knightMask = KnightMovement.GetTargetVectors(square).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.Square.ToMask());
                    var rookMask = RookMovement.GetTargetVectors(square).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.Square.ToMask());
                    var kingMask = KingMovement.GetTargetVectors(square, colour.OpponentColour()).SelectMany(v => v).Where(v => !v.Flags.HasFlag(SpecialMove.CastleQueen) && !v.Flags.HasFlag(SpecialMove.CastleKing)).Aggregate(0ul, (a, b) => a | b.Square.ToMask());

                    // TODO: There should probably be some enpassant handling somewhere, maybe in here?
                    var pawnMask = 0ul;

                    if (colour == PieceColour.White && square.Rank < 6)
                    {
                        if (Square.TryCreate(square.File - 1, square.Rank + 1, out var upLeft))
                        {
                            pawnMask |= upLeft.ToMask();
                        }
                        if (Square.TryCreate(square.File + 1, square.Rank + 1, out var upRight))
                        {
                            pawnMask |= upRight.ToMask();
                        }
                    }
                    else if (colour == PieceColour.Black && square.Rank > 1)
                    {
                        if (Square.TryCreate(square.File - 1, square.Rank - 1, out var downLeft))
                        {
                            pawnMask |= downLeft.ToMask();
                        }
                        if (Square.TryCreate(square.File + 1, square.Rank - 1, out var downRight))
                        {
                            pawnMask |= downRight.ToMask();
                        }
                    }

                    threatMasks[colour].Add(square, new ThreatMask(pawnMask, rookMask, knightMask, bishopMask, queenMask, kingMask));
                }
            }
        }
    }
}
