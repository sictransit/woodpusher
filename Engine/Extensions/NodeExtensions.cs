namespace SicTransit.Woodpusher.Engine.Extensions
{
    public static class NodeExtensions
    {
        public static int? MateIn(this Node n)
        {
            var mateIn = Math.Abs(Math.Abs(n.Score) - Patzer.MATE_SCORE);

            if (mateIn < 80)
            {
                mateIn += n.Sign == -1 ? 1 : 0;
                var mateSign = n.AbsoluteScore > 0 ? 1 : -1;

                return mateSign * mateIn / 2;
            }

            return null;
        }
    }
}
