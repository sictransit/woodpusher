namespace SicTransit.Woodpusher.Model.Extensions
{
    public static class CharExtensions
    {
        public static int ToFile(this char c)
        {
            return c - 'a';
        }

        public static int ToRank(this char c)
        {
            return c - '1';
        }

    }
}
