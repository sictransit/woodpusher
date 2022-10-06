using Newtonsoft.Json;
using Serilog;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class OpeningBook
    {
        // Credit: https://github.com/lichess-org/chess-openings

        private Dictionary<string, HashSet<string>> book = new();

        private static readonly string bookFilename = Path.Combine(@"Resources\eco.json");

        public OpeningBook()
        {
            LoadFromFile();
        }

        public void LoadFromFile(string? filename = null)
        {
            filename ??= bookFilename;

            if (!File.Exists(filename))
            {
                Log.Error($"Unable to open the opening book: {filename}");
            }
            else
            {
                book = JsonConvert.DeserializeObject<Dictionary<string, HashSet<string>>>(File.ReadAllText(filename))!;
            }
        }

        public void SaveToFile(string? filename = null)
        {
            filename ??= bookFilename;

            var json = JsonConvert.SerializeObject(book, Formatting.Indented);

            File.WriteAllText(filename, json);
        }

        public void AddMove(string hash, Move move)
        {
            if (book.TryGetValue(hash, out var moves))
            {
                moves.Add(move.ToAlgebraicMoveNotation());
            }
            else
            {
                book.Add(hash, new HashSet<string>() { move.ToAlgebraicMoveNotation() });
            }
        }

        public IEnumerable<AlgebraicMove> GetMoves(string hash)
        {
            if (book.TryGetValue(hash, out var moves))
            {
                return moves.Select(m => AlgebraicMove.Parse(m));
            }

            return Enumerable.Empty<AlgebraicMove>();
        }
    }
}
