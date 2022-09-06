namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class CastlingKingMove : CastlingMove
    {
        public CastlingKingMove(string raw) : base(raw)
        {
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] castle kingside";
        }
    }
}
