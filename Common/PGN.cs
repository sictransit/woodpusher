using Serilog;
using SicTransit.Woodpusher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Common
{
    public class PGN
    {
        private readonly Dictionary<string, string> tags = new();        

        private PGN()
        { }

        public static PGN Parse(string s)
        {
            var pgn = new PGN();

            using (var reader = new StringReader(s))
            {
                string line;
                var moveIndex = 1;
                while ((line = reader.ReadLine()) != null)
                {
                    if (TryParseTag(line, out var tag))
                    {
                        pgn.tags.Add(tag.Key, tag.Value);
                    }
                    else if (TryParseMoves(line, moveIndex, out var moves))
                    {
                    }
                    else
                    {
                        Log.Debug($"ignored line: {line}");
                    }
                }
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

        private static bool TryParseMoves(string s, int index, out IEnumerable<PgnMove> moves)
        {


            moves = Enumerable.Empty<PgnMove>();

            s = RemoveComments(s);
            s = RemoveVariations(s);
            
            Log.Debug($"parsing moves: {s}");
            return false;
        }

        private static string RemoveComments(string s)
        {
            var r = new Regex(@"\{[^\{]+?\}");
            return r.Replace(s, string.Empty, int.MaxValue);
        }

        private static string RemoveVariations(string s)
        {
            var r = new Regex(@"\([^\(]+?\)");
            return r.Replace(s, string.Empty, int.MaxValue);
        }

        private struct PgnMove
        { 

        }

    }
}
