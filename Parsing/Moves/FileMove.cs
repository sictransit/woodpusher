using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class FileMove : PgnMove
    {
        private readonly int file;
        private readonly Square square;

        public FileMove(int file, Square square)
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
            return $"from file {file}, {square}";
        }

    }
}
