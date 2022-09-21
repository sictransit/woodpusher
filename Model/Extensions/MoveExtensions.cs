namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class MoveExtensions
    {
        public static string ToAlgebraicMoveNotation(this Move move)
        {
            var promotion = move.Target.PromotionType == Enums.PieceType.None ? string.Empty : char.ToUpperInvariant(move.Target.PromotionType.ToChar()).ToString();

            return $"{move.Position.Square.ToAlgebraicNotation()}{move.Target.Square.ToAlgebraicNotation()}{promotion}";
        }
    }
}
