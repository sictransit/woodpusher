namespace SicTransit.Woodpusher.Engine
{
    internal class TimeManager
    {

        // y = 0.577e3.2472x

        private int timeLeft;

        private readonly Dictionary<int, List<int>> searches = new();

        public TimeManager()
        {

        }

        public void Clear()
        {
            searches.Clear();
            timeLeft = 0;
        }

        public void Reset(int timeLeft)
        {
            this.timeLeft = timeLeft;
        }

        public void AddNode(Node node)
        {
            // maybe divide time by number of cores?
        }

        public bool HasTime(Node node)
        {
            // TODO: clever calculation to see if we can fit a node
            // consider number of CPU's
            // decrease timeLeft for every TRUE returned

            return true;
        }

    }
}
