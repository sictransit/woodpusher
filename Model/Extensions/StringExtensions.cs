using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class StringExtensions
    {
        public static bool IsAlgebraicNotation(this string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.Length != 2)
            {
                return false;
            }

            return Regex.IsMatch(s, "^[a-h]{1}[1-8]{1}$");
        }

        public static bool IsNothing(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException($"'{nameof(s)}' cannot be null or whitespace.", nameof(s));
            }

            return s.Equals("-", StringComparison.InvariantCulture);
        }
    }
}
