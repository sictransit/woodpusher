using Serilog;
using SicTransit.Woodpusher.Common.Parsing.Enum;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Common.Parsing.Extensions;
using SicTransit.Woodpusher.Common.Parsing.Moves;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Common.Parsing
{
    public class PortableGameNotation
    {
        private static readonly Regex ResultRegex = new(@"(1\-0|0-1|1\/2.1\/2|\*)", RegexOptions.Compiled);
        private static readonly Regex MoveRegex = new(@"^[abcdefgh12345678RNBQKPO\-x=\?!]+$", RegexOptions.Compiled);
        private static readonly Regex IndexRegex = new(@"\d+\.+", RegexOptions.Compiled);

        public IDictionary<string, string> Tags { get; }

        public IList<PgnMove> PgnMoves { get; }

        public Result Result { get; private set; }

        public string Source { get; }

        public int? WhiteElo => GetElo(Piece.White);

        public int? BlackElo => GetElo(Piece.None);

        private int? GetElo(Piece color)
        {
            var key = color.Is(Piece.White) ? "WhiteElo" : "BlackElo";

            return Tags.TryGetValue(key, out var value) && int.TryParse(value, out var elo) ? elo : null;
        }

        private PortableGameNotation(string source)
        {
            Tags = new Dictionary<string, string>();
            PgnMoves = new List<PgnMove>();
            Result = Result.Ongoing;
            Source = source;
        }

        public static PortableGameNotation Parse(string s)
        {
            Log.Debug($"Parsing PGN:\n{s}");

            var pgn = new PortableGameNotation(s);

            var sb = new StringBuilder();

            using (var reader = new StringReader(s))
            {
                while (reader.ReadLine() is { } line)
                {
                    if (TryParseTag(line, out var tag))
                    {
                        pgn.Tags.Add(tag.Key, tag.Value);
                    }
                    else
                    {
                        sb.Append(line + ' ');
                    }
                }
            }

            var moveSection = sb.ToString().RemoveComments().RemoveVariations().RemoveAnnotations();

            foreach (var move in ParseMoves(moveSection))
            {
                pgn.PgnMoves.Add(move);
            }

            pgn.Result = ParseResult(moveSection);

            return pgn;
        }

        private static bool TryParseTag(string s, out KeyValuePair<string, string> tag)
        {
            tag = default;

            Regex tagsRegex = new(@"\[\s*(.+?)\s*\""(.*?)\""\s*\]");

            var match = tagsRegex.Match(s);

            if (!match.Success)
            {
                return false;
            }

            Log.Debug(match.ToString());

            tag = new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value);

            return true;
        }

        private static Result ParseResult(string s)
        {
            var match = ResultRegex.Match(s);

            if (match.Success)
            {
                switch (match.Value)
                {
                    case "1-0":
                        return Result.WhiteWin;
                    case "0-1":
                        return Result.BlackWin;
                    case "1/2-1/2":
                    case "1/2 1/2":
                        return Result.Draw;
                }
            }

            return Result.Ongoing;
        }


        private static IEnumerable<PgnMove> ParseMoves(string s)
        {
            var movesPart = ResultRegex.Split(s);

            var moves = IndexRegex.Split(movesPart[0]).Where(p => !string.IsNullOrWhiteSpace(p));

            foreach (var move in moves)
            {
                var plys = move.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var ply in plys)
                {
                    if (MoveRegex.IsMatch(ply))
                    {
                        yield return PgnMove.Parse(ply);
                    }
                    else
                    {
                        throw new PgnParsingException(s, $"Failed to parse ply: {ply}");
                    }
                }
            }
        }
    }
}
