using SicTransit.Woodpusher.Common.Movement;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class Attacks
    {
        private readonly Dictionary<Piece, ThreatMasks> threatMasks = new();

        public Attacks()
        {
            Initialize();
        }

        public ThreatMasks GetThreatMask(Piece piece) => threatMasks[piece];

        private void Initialize()
        {
            foreach (var pieceType in PieceExtensions.Types)
            {
                foreach (var color in PieceExtensions.Colors)
                {
                    foreach (var square in SquareExtensions.AllSquares)
                    {
                        var queenMask = QueenMovement.GetTargetVectors(color.SetSquare(square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                        var bishopMask = BishopMovement.GetTargetVectors(color.SetSquare(square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                        var knightMask = KnightMovement.GetTargetVectors(color.SetSquare(square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                        var rookMask = RookMovement.GetTargetVectors(color.SetSquare(square)).SelectMany(v => v).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());
                        var kingMask = KingMovement.GetTargetVectors(color.OpponentColor().SetSquare(square)).SelectMany(v => v).Where(v => !v.Flags.HasFlag(SpecialMove.CastleQueen) && !v.Flags.HasFlag(SpecialMove.CastleKing)).Aggregate(0ul, (a, b) => a | b.GetTarget().ToMask());

                        var pawnMask = 0ul;

                        switch (color)
                        {
                            case Piece.White when square.Rank < 6:
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
                            case Piece.None when square.Rank > 1:
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

                        threatMasks.Add(pieceType | color.SetSquare(square), new ThreatMasks(pawnMask, rookMask, knightMask, bishopMask, queenMask, kingMask));
                    }
                }
            }
        }
    }
}
