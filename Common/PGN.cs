using Serilog;
using SicTransit.Woodpusher.Common.Extensions;
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

            if (TryParseMoves(moveSection, out var moves))
            {
                foreach (var move in moves)
                {
                    Log.Debug($"move: {move}");
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

        private static bool TryParseMoves(string s, out IEnumerable<PgnMove> moves)
        {
            moves = Enumerable.Empty<PgnMove>();
            
            Log.Debug($"parsing moves: {s}");

            return false;
        }


        private struct PgnMove
        { 

        }

    }
}
