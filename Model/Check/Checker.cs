using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Model.Movement;

namespace SicTransit.Woodpusher.Model.Check
{
    public class Checker
    {
        private readonly Dictionary<PieceColour, Dictionary<Square, CheckMask>> checkMasks = new();

        public Checker()
        {
            Initialize();
        }

        public CheckMask GetCheckMask(PieceColour pieceColour, Square square) => checkMasks[pieceColour][square];

        private void Initialize()
        {
            foreach (var colour in new[] { PieceColour.White, PieceColour.Black })
            {
                checkMasks.Add(colour, new Dictionary<Square, CheckMask>());

                var squares = Enumerable.Range(0, 8).Select(f => Enumerable.Range(0, 8).Select(r => new Square(f, r))).SelectMany(x => x).ToList();

                foreach (var square in squares)
                {
                    var queenMask = QueenMovement.GetTargetVectors(square).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.Square.ToMask());
                    var bishopMask = BishopMovement.GetTargetVectors(square).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.Square.ToMask());
                    var knightMask = KnightMovement.GetTargetVectors(square).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.Square.ToMask());
                    var rookMask = RookMovement.GetTargetVectors(square).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.Square.ToMask());
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

                    checkMasks[colour].Add(square, new CheckMask(pawnMask, rookMask, knightMask, bishopMask, queenMask));
                }
            }
        }
    }
}
