using Serilog;
using SicTransit.Woodpusher.Parsing.Extensions;
using System.Text;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Parsing
{
    public class PortableGameNotation
    {
        private readonly Dictionary<string, string> tags = new();

        private PortableGameNotation()
        { }

        public static PortableGameNotation Parse(string s)
        {
            var pgn = new PortableGameNotation();

            var sb = new StringBuilder();

            using (var reader = new StringReader(s))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (TryParseTag(line, out var tag))
                    {
                        pgn.tags.Add(tag.Key, tag.Value);
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
                Log.Debug($"move: {move}");
            }

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

        private static IEnumerable<string> ParseMoves(string s)
        {
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var indexRegex = new Regex(@"(\d+)\.");
            var moveRegex = new Regex(@"[abcdefgh12345678RNBQKPO\-x]+");
            var resultRegex = new Regex(@"0\-0|1-1|1\/2-1\/2");

            foreach (var part in parts)
            {
                var indexMatch = indexRegex.Match(part);

                if (indexMatch.Success)
                {
                    continue;
                }

                var moveMatch = moveRegex.Match(part);

                if (moveMatch.Success && !resultRegex.IsMatch(part))
                {
                    yield return moveMatch.Groups[0].Value;
                }
            }
        }
    }
}
