namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class MoveExtensions
    {
        public static string ToAlgebraicMoveNotation(this Move move)
        {
            var promotion = move.PromotionType == Enums.Piece.None ? string.Empty : char.ToUpperInvariant(move.PromotionType.ToChar()).ToString();

            return $"{move.Piece.GetSquare().ToAlgebraicNotation()}{move.GetTarget().ToAlgebraicNotation()}{promotion}";
        }
    }
}
