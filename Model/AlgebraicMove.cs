using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public class AlgebraicMove
    {
        private AlgebraicMove(Square from, Square to, PieceType promotion)
        {
            From = from;
            To = to;
            Promotion = promotion;
        }

        public AlgebraicMove(Move move) : this(move.Position.Square, move.Target, move.PromotionType)
        {
        }

        public Square From { get; }
        public Square To { get; }
        public PieceType Promotion { get; }

        public string Notation => $"{From.ToAlgebraicNotation()}{To.ToAlgebraicNotation()}{(Promotion != PieceType.None ? char.ToLowerInvariant(Promotion.ToChar()) : string.Empty)}";

        public static AlgebraicMove Parse(string notation)
        {
            if (!string.IsNullOrEmpty(notation) && notation.Length is > 3 and < 6)
            {
                var from = notation[..2];
                var to = notation[2..4];

                if (from.IsAlgebraicNotation() && to.IsAlgebraicNotation())
                {
                    var pieceType = PieceType.None;

                    if (notation.Length == 5)
                    {
                        pieceType = char.ToUpperInvariant(notation[4]).ToPieceType();
                    }

                    return new AlgebraicMove(new Square(from), new Square(to), pieceType);
                }
            }

            throw new ArgumentException($"unable to parse: {notation}", nameof(notation));
        }

        public override string ToString()
        {
            return Notation;
        }
    }
}
