using Newtonsoft.Json;
using Serilog;
using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Lookup
{
    public class OpeningBook
    {
        // Credit: https://github.com/lichess-org/chess-openings

        private Dictionary<ulong, Dictionary<string, int>> book = new();

        private readonly Random random = new();

        public string BookFilename { get; }

        public OpeningBook(Piece color, bool startEmpty = false)
        {
            BookFilename = Path.Combine(@"Resources", $"openings.{(color.Is(Piece.White) ? "white" : "black")}.json");

            if (!startEmpty)
            {
                LoadFromFile();
            }
        }

        //public void Prune(int keepTopPercent)
        //{
        //    foreach (var hash in book.Keys)
        //    {
        //        var moves = book[hash];

        //        foreach (var move in moves.OrderByDescending(m => m.Value).Skip(Math.Max(1,moves.Count/keepTopPercent)))
        //        {
        //            moves.Remove(move.Key);
        //        }
        //    }

        //    foreach (var entry in book.OrderByDescending(e => e.Value.Sum(x => x.Value)).Skip(book.Count / keepTopPercent))
        //    {
        //        book.Remove(entry.Key);
        //    }
        //}

        public void LoadFromFile(string? filename = null)
        {
            filename ??= BookFilename;

            if (!File.Exists(filename))
            {
                Log.Error($"Unable to open the opening book: {filename}");
            }
            else
            {
                var loadedBook = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<string, int>>>(File.ReadAllText(filename))!;

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

        public Move? GetMove(IBoard board)
        {
            if (book.TryGetValue(board.Hash, out Dictionary<string, int>? moves))
            {
                var sum = moves.Sum(m => m.Value);
                var index = random.Next(sum);
                var count = 0;
                string algebraicMove = string.Empty;

                foreach (var move in moves.OrderByDescending(m=>m.Value))
                {
                    count += move.Value;
                    if (count > index)
                    {
                        algebraicMove = move.Key;

                        break;
                    }
                }

                foreach(var move in board.GetLegalMoves())
                {
                    if (move.ToAlgebraicMoveNotation() == algebraicMove)
                    {
                        return move;
                    }
                }

            }

            return null;
        }

        public Move? GetTheoryMove(IBoard board)
        {
            foreach (var newBoard in board.PlayLegalMoves())
            {
                var theoryMove = GetMove(newBoard);

                if (theoryMove != null)
                {
                    return theoryMove;
                }   
            }

            return null;
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
