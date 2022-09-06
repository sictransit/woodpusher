using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class SimplePieceMove : PgnMove
    {
        private readonly PieceType pieceType;
        private readonly Square square;

        public SimplePieceMove(string raw, PieceType pieceType, Square square) : base(raw)
        {
            this.pieceType = pieceType;
            this.square = square;
        }

        protected override Move CreateMove(IEngine engine)
        {
            return default;
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] {pieceType} to {square}";
        }
    }
}
