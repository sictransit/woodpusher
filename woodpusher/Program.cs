using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Engine;

namespace SicTransit.Woodpusher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var patzer = new Patzer();

            patzer.Initialize(FEN.StartingPosition);

            patzer.GetValidMoves();
        }
    }
}