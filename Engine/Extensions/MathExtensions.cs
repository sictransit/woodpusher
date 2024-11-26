namespace SicTransit.Woodpusher.Engine.Extensions
{
    public static class MathExtensions
    {
        public static int ApproximateNextDepthTime(List<(int depth, long time)> progress, int nextDepth)
        {
            int n = progress.Count;

            double[] logTimes = new double[n];

            // Perform linear regression on (depth, logTime).
            double sumX = 0;
            double sumLogY = 0;
            double sumXLogY = 0;
            double sumXSquared = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += progress[i].depth;
                logTimes[i] = Math.Log(progress[i].time);
                sumLogY += logTimes[i];
                sumXLogY += progress[i].depth * logTimes[i];
                sumXSquared += progress[i].depth * progress[i].depth;
            }

            // Calculate the slope (b) and intercept (ln(a)) of the linear fit
            double b = (n * sumXLogY - sumX * sumLogY) / (n * sumXSquared - sumX * sumX);
            double lnA = (sumLogY - b * sumX) / n;

            // Convert ln(a) back to a
            double a = Math.Exp(lnA);

            return (int)(a * Math.Exp(b * (nextDepth)));
        }
    }
}
