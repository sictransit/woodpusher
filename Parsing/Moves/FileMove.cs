using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;

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
            return default;
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] from file {file}, {square}";
        }

    }
}
