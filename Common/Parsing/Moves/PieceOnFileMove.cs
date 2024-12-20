﻿using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public class PieceOnFileMove : PgnMove
    {
        private readonly Piece pieceType;
        private readonly int file;
        private readonly Square square;
        private readonly Piece promotionType;

        public PieceOnFileMove(Piece pieceType, int file, Square square, Piece promotionType)
        {
            this.pieceType = pieceType;
            this.file = file;
            this.square = square;
            this.promotionType = promotionType;
        }

        public override string ToString()
        {
            return $"[{base.ToString()}] {pieceType} from file {file} to {square}";
        }

        protected override Move CreateMove(IEngine engine)
        {
            var board = engine.Board;

            var pieces = board.GetPieces(board.ActiveColor, pieceType);

            foreach (var piece in pieces.Where(p => p.GetSquare().File == file))
            {
                var legalMove = board.GetLegalMoves(piece).SingleOrDefault(move => move.GetTarget().Equals(square) && move.PromotionType == promotionType);

                if (legalMove != null)
                {
                    return legalMove;
                }
            }

            throw new PgnParsingException(Raw, "Unable to find a legal move to match.");
        }
    }
}
