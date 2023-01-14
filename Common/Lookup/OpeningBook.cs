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
                var loadedBook = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<string, uint>>>(File.ReadAllText(filename))!;

                foreach (var hash in loadedBook)
                {
                    foreach (var move in hash.Value)
                    {
                        AddMove(hash.Key, move.Key, move.Value);
                    }
                }
            }
        }

        public void SaveToFile(string? filename = null)
        {
            filename ??= BookFilename;

            var json = JsonConvert.SerializeObject(book, Formatting.Indented);

            File.WriteAllText(filename, json);
        }

        public void AddMove(ulong hash, string move, uint count = 1)
        {
            if (book.TryGetValue(hash, out var moves))
            {
                if (moves.ContainsKey(move))
                {
                    moves[move] += count;
                }
                else
                {
                    moves.Add(move, count);
                }
            }
            else
            {
                var moveEntry = new Dictionary<string, uint>
                {
                    { move, count }
                };

                book.Add(hash, moveEntry);
            }
        }

        public void AddMove(ulong hash, Move move) => AddMove(hash, move.ToAlgebraicMoveNotation());

        public IEnumerable<AlgebraicMove> GetMoves(ulong hash)
        {
            if (book.TryGetValue(hash, out var moves))
            {
                return moves.OrderByDescending(m => m.Value).Select(m => AlgebraicMove.Parse(m.Key));
            }

            return Enumerable.Empty<AlgebraicMove>();
        }
    }
}
