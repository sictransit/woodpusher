using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Parsing.Exceptions;

namespace SicTransit.Woodpusher.Parsing.Moves
{
    public class PieceOnFileMove : PgnMove
    {
        private readonly PieceType pieceType;
        private readonly int file;
        private readonly Square square;

        public PieceOnFileMove(string raw, PieceType pieceType, int file, Square square) : base(raw)
        {
            this.pieceType = pieceType;
            this.file = file;
            this.square = square;
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] {pieceType} from file {file} to {square}";
        }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var positions = board.GetPositions(board.ActiveColour, pieceType, file);

            var moves = positions.Select(p => new Move(p, new Target(square)));

            var validMoves = moves.Where(m => engine.IsValidMove(m) && m.Target.Equals(square));

            if (validMoves.Count() != 1)
            {
                throw new PgnParsingException(Raw, "unable to find one unique valid move to match");
            }

            return new Move(validMoves.Single().Position, new Target(square));
        }
    }
}
