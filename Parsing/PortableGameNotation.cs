using Serilog;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Parsing.Extensions;
using SicTransit.Woodpusher.Parsing.Moves;
using System.Text;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Parsing
{
    public class PortableGameNotation
    {
        private static Regex ResultRegex = new(@"(0\-0|1-1|1\/2-1\/2|\*)", RegexOptions.Compiled);

        public IDictionary<string, string> Tags { get; private set; }

        public IList<PgnMove> PgnMoves { get; private set; }

        public Result Result { get; private set; }


        private PortableGameNotation()
        {
            Tags = new Dictionary<string, string>();
            PgnMoves = new List<PgnMove>();
            Result = Result.Ongoing;
        }

        public static PortableGameNotation Parse(string s)
        {
            Log.Debug($"Parsing PGN:\n{s}");

            var pgn = new PortableGameNotation();

            var sb = new StringBuilder();

            using (var reader = new StringReader(s))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
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

            var moveSection = sb.ToString().RemoveComments().RemoveVariations();

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

            Regex tagsRegex = new(@"\[\s*(.+?)\s*\""(.+)\""\s*\]");

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
                        return Result.Draw;
                }
            }

            return Result.Ongoing;
        }


        private static IEnumerable<PgnMove> ParseMoves(string s)
        {
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var indexRegex = new Regex(@"(\d+)\.");
            var moveRegex = new Regex(@"[abcdefgh12345678RNBQKPO\-x]+");


            foreach (var part in parts)
            {
                var indexMatch = indexRegex.Match(part);

                if (indexMatch.Success)
                {
                    continue;
                }

                var moveMatch = moveRegex.Match(part);

                if (moveMatch.Success && !ResultRegex.IsMatch(part))
                {
                    yield return PgnMove.Parse(part);
                }
            }
        }
    }
}
