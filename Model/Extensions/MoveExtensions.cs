namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class MoveExtensions
    {
        public static string ToAlgebraicMoveNotation(this Move move)
        {
            return $"{move.Position.Square.ToAlgebraicNotation()}{move.Target.Square.ToAlgebraicNotation()}";
        }
    }
}
