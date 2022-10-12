using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public class AlgebraicMove
    {
        private AlgebraicMove(Square from, Square to, Pieces promotion)
        {
            From = from;
            To = to;
            Promotion = promotion;
        }

        public AlgebraicMove(Move move) : this(move.Piece.GetSquare(), move.GetTarget(), move.PromotionType)
        {
        }

        public Square From { get; }
        public Square To { get; }
        public Pieces Promotion { get; }

        public string Notation => $"{From.ToAlgebraicNotation()}{To.ToAlgebraicNotation()}{(Promotion != Pieces.None ? char.ToLowerInvariant(Promotion.ToChar()) : string.Empty)}";

        public static AlgebraicMove Parse(string notation)
        {
            if (!string.IsNullOrEmpty(notation) && notation.Length is > 3 and < 6)
            {
                var from = notation[..2];
                var to = notation[2..4];

                if (from.IsAlgebraicNotation() && to.IsAlgebraicNotation())
                {
                    var pieceType = Pieces.None;

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

        public override bool Equals(object? obj)
        {
            return obj is AlgebraicMove move &&
                   EqualityComparer<Square>.Default.Equals(From, move.From) &&
                   EqualityComparer<Square>.Default.Equals(To, move.To) &&
                   Promotion == move.Promotion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To, Promotion);
        }
    }
}
