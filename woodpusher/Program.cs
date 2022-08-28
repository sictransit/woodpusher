using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model;

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