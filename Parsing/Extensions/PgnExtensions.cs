using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Parsing.Extensions
{
    public static class PgnExtensions
    {
        public static string RemoveComments(this string s)
        {
            var r = new Regex(@"\{[^\{]+?\}", RegexOptions.Singleline);

            return RepeatReplace(r, s);
        }

        public static string RemoveVariations(this string s)
        {
            var r = new Regex(@"\([^\(]+?\)", RegexOptions.Singleline);

            return RepeatReplace(r, s);
        }

        public static string RemoveAnnotations(this string s)
        {
            char[] separators = { '?', '!', '+', '#' };

            var parts = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            return string.Concat(parts);
        }

        private static string RepeatReplace(Regex regex, string s)
        {
            var src = s;

            while (true)
            {
                var dst = regex.Replace(src, string.Empty, int.MaxValue);

                if (src.Equals(dst, StringComparison.OrdinalIgnoreCase))
                {
                    return dst;
                }

                src = dst;
            }
        }
    }
}
