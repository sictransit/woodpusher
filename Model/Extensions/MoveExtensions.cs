namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class MoveExtensions
    {
        public static string ToAlgebraicMoveNotation(this Move move)
        {
            var promotion = move.PromotionType == Enums.PieceType.None ? string.Empty : char.ToUpperInvariant(move.PromotionType.ToChar()).ToString();

            return $"{move.Position.Square.ToAlgebraicNotation()}{move.Target.ToAlgebraicNotation()}{promotion}";
        }
    }
}
