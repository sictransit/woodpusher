using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SicTransit.Woodpusher.Model
{
    public struct PiecePosition
    {
        public PiecePosition(Piece piece, Square square)
        {
            Piece = piece;
            Square = square;
        }

        public Piece Piece { get; }
        public Square Square { get; }
    }
}
