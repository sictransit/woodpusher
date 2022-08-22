using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class StringExtensions
    {
        public static bool IsAlgebraicNotation(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException($"'{nameof(s)}' cannot be null or whitespace.", nameof(s));
            }

            return Regex.IsMatch(s, "^[a-h]{1}[1-8]{1}$");
        }
    }
}
