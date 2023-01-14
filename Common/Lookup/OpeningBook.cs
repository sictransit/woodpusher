using Newtonsoft.Json;
using Serilog;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class OpeningBook
    {
        // Credit: https://github.com/lichess-org/chess-openings

        private Dictionary<ulong, Dictionary<string, uint>> book = new();

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
                book = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<string, uint>>>(File.ReadAllText(filename))!;
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
            var key = move.ToAlgebraicMoveNotation();

            if (book.TryGetValue(hash, out var moves))
            {
                if (moves.ContainsKey(key))
                {
                    moves[key]++;
                }
                else
                {
                    moves.Add(key, 1);
                }                
            }
            else
            {
                var moveEntry = new Dictionary<string, uint>
                {
                    { key, 1 }
                };

                book.Add(hash, moveEntry);
            }
        }

        public IEnumerable<AlgebraicMove> GetMoves(ulong hash)
        {
            if (book.TryGetValue(hash, out var moves))
            {
                return moves.OrderByDescending(m => m.Value).Select(m=>AlgebraicMove.Parse(m.Key));
            }

            return Enumerable.Empty<AlgebraicMove>();
        }
    }
}
