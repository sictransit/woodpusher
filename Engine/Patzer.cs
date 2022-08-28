using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;

namespace SicTransit.Woodpusher.Engine
{
    public class Patzer
    {
        public Board Board { get; private set; }
        public Piece ActiveColour { get; private set; }
        public Castlings Castlings { get; private set; }

        public Patzer()
        {
            Logging.EnableLogging(Serilog.Events.LogEventLevel.Debug);
        }


        public void Initialize(string position)
        {
            var fen = FEN.Parse(position);

            Board = fen.Board;
            ActiveColour = fen.ActiveColour;
            Castlings = fen.Castlings;
        }

        public void GetValidMoves()
        {
            foreach (var position in Board.GetPositions(ActiveColour))
            {
                Log.Debug(position.ToString());
            }
        }
    }
}