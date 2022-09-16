using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Parsing.Exceptions;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class FileMove : PgnMove
    {
        private readonly int file;
        private readonly Square square;

        public FileMove(string raw, int file, Square square) : base(raw)
        {
            this.file = file;
            this.square = square;
        }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var positions = board.GetPositions(board.ActiveColour, file);

            foreach (var position in positions)
            {
                var move = board.GetValidMovesFromPosition(position).SingleOrDefault(m => m.Target.Square.Equals(square));

                if (move != null)
                {
                    return move;
                }
            }

            throw new PgnParsingException(Raw, "unable to find a valid move to match");
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] from file {file}, {square}";
        }

    }
}
