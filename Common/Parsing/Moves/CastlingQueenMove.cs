using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class CastlingQueenMove : CastlingMove
    {
        public CastlingQueenMove(string raw) : base(raw)
        {
        }

        protected override Castlings Castling => Castlings.Queenside;

        public override string ToString()
        {
            return $"[{base.ToString()}] castle queenside";
        }
    }
}
