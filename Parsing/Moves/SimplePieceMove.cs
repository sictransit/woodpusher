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

        protected override void Apply(IEngine engine)
        {
            Log.Debug($"applying {this}");
        }

        public override string ToString()
        {
            return $"{pieceType} to {square}";
        }
    }
}
