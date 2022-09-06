using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class SimplePieceMove : PgnMove
    {
        private readonly PieceType pieceType;
        private readonly Square square;

        public SimplePieceMove(PieceType pieceType, Square square)
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
            return $"{pieceType} to {square}";
        }
    }
}
