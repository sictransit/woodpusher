using Newtonsoft.Json;
using Serilog;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class OpeningBook
    {
        // Credit: https://github.com/lichess-org/chess-openings

        private Dictionary<ulong, HashSet<string>> book = new();

        private static readonly string BookFilename = Path.Combine(@"Resources\eco.json");

        public OpeningBook(bool startEmpty = false)
        {
            if (!startEmpty)
            {
                LoadFromFile();
            }
        }

        public void LoadFromFile(string? filename = null)
        {
            filename ??= BookFilename;

            if (!File.Exists(filename))
            {
                Log.Error($"Unable to open the opening book: {filename}");
            }
            else
            {
                book = JsonConvert.DeserializeObject<Dictionary<ulong, HashSet<string>>>(File.ReadAllText(filename))!;
            }
        }

        public void SaveToFile(string? filename = null)
        {
            filename ??= BookFilename;

            var json = JsonConvert.SerializeObject(book, Formatting.Indented);

            File.WriteAllText(filename, json);
        }

        public void AddMove(ulong hash, Move move)
        {
            if (book.TryGetValue(hash, out var moves))
            {
                moves.Add(move.ToAlgebraicMoveNotation());
            }
            else
            {
                book.Add(hash, new HashSet<string> { move.ToAlgebraicMoveNotation() });
            }
        }

        public IEnumerable<AlgebraicMove> GetMoves(ulong hash)
        {
            return book.TryGetValue(hash, out var moves) ? moves.Select(AlgebraicMove.Parse) : Enumerable.Empty<AlgebraicMove>();
        }
    }
}
