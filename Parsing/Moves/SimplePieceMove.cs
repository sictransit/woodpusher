using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class SimplePieceMove : PgnMove
    {
        private readonly Piece piece;
        private readonly Square square;

        public SimplePieceMove(Piece piece, Square square)
        {
            this.piece = piece;
            this.square = square;
        }

        protected override void Apply(IEngine engine)
        {
            Log.Debug($"applying {this}");
        }

        public override string ToString()
        {
            return $"{piece} to {square}";
        }
    }
}
