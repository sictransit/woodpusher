namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class CastlingQueenMove : CastlingMove
    {
        public CastlingQueenMove(string raw) : base(raw)
        {
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] castle queenside";
        }
    }
}
