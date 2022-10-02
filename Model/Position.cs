using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Model
{
    public class Position
    {
        public Position(Piece piece, Square current) : this(piece, current.ToMask())
        { }

        public Position(Piece piece, ulong current)
        {
            Piece = piece;
            Current = current;
        }


        public Piece Piece { get; }

        public ulong Current { get; }

        public Square Square => Current.ToSquare();

        public override string ToString()
        {
            return $"{Piece.Type.ToChar()}{Square}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Position position &&
                   EqualityComparer<Piece>.Default.Equals(Piece, position.Piece) &&
                   Current == position.Current;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Piece, Current);
        }
    }
}
