using MessagePack;
using Serilog;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class OpeningBook
    {
        // Credit: https://github.com/lichess-org/chess-openings

        private Dictionary<ulong, Dictionary<string, int>> book = new();

        public string BookFilename { get; }

        public OpeningBook(Piece color, bool startEmpty = false)
        {
            BookFilename = Path.Combine(@"Resources", $"openings.{(color.Is(Piece.White) ? "white" : "black")}.bin");

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
                var loadedBook = MessagePackSerializer.Deserialize<Dictionary<ulong, Dictionary<string, int>>>(File.ReadAllBytes(filename));

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

            var bytes = MessagePackSerializer.Serialize(book);

            File.WriteAllBytes(filename, bytes);
        }

        private void AddMove(ulong hash, string move, int count = 1)
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
                var moveEntry = new Dictionary<string, int>
                {
                    { move, count }
                };

                book.Add(hash, moveEntry);
            }
        }

        public void AddMove(ulong hash, Move move) => AddMove(hash, move.ToAlgebraicMoveNotation());

        public IEnumerable<OpeningBookMove> GetMoves(ulong hash)
        {
            if (book.TryGetValue(hash, out var moves))
            {
                return moves.Select(m => new OpeningBookMove(AlgebraicMove.Parse(m.Key), m.Value));
            }

            return Enumerable.Empty<OpeningBookMove>();
        }

        public class OpeningBookMove
        {
            public int Count { get; }

            public AlgebraicMove Move { get; }

            public OpeningBookMove(AlgebraicMove move, int Count)
            {
                Move = move;
                this.Count = Count;
            }
        }
    }
}
