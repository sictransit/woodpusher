using Serilog;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Common.Parsing
{
    public static class ForsythEdwardsNotation
    {
        public const string StartingPosition = @"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static Board Parse(string fen)
        {
            if (fen is null)
            {
                throw new ArgumentNullException(nameof(fen));
            }

            var parts = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 4)
            {
                throw new FenParsingException(fen, "parts should be >= 4");
            }

            var (white, black) = ParseBoard(parts[0]);

            var activeColor = ParseActiveColor(parts[1]);

            var castling = ParseCastling(parts[2]);

            var enPassantTarget = ParseEnPassantTarget(parts[3]);

            var halfmoveClock = parts.Length > 4 ? ParseHalfmoveClock(parts[4]) : 0;

            var fullmoveNumber = parts.Length > 5 ? ParseFullmoveNumber(parts[5]) : 1;

            var counters = new Counters(activeColor, castling, enPassantTarget?.ToMask() ?? 0, halfmoveClock, fullmoveNumber, null, Piece.None);

            return new Board(white, black, counters);
        }

        private static (Bitboard, Bitboard) ParseBoard(string s)
        {
            // TODO: regex for validating FEN board setup

            var parts = s.Split('/');

            if (parts.Length != 8)
            {
                throw new FenParsingException(s, "board setup should be eight parts, separated by '/'");
            }

            var rank = 7;

            Bitboard white = new(Piece.White);
            Bitboard black = new(Piece.None);

            foreach (var part in parts)
            {
                var file = 0;

                foreach (var c in part)
                {
                    if (char.IsDigit(c))
                    {
                        file += c - '0';
                    }
                    else
                    {
                        var square = new Square(file++, rank);

                        var piece = c.ToPiece();

                        if (piece.Is(Piece.White))
                        {
                            white = white.Toggle(piece.SetSquare(square));
                        }
                        else
                        {
                            black = black.Toggle(piece.SetSquare(square));
                        }
                    }
                }

                rank--;
            }

            return (white, black);
        }

        public static string Export(Board board)
        {
            var parts = new List<string>();

            var boardParts = new List<string>();

            var pieces = board.GetPieces().ToDictionary(p => p.GetSquare(), p => p);

            for (var rank = 7; rank >= 0; rank--)
            {
                var emptySquares = 0;

                var fileBuilder = new StringBuilder();

                for (var file = 0; file < 8; file++)
                {
                    if (pieces.TryGetValue(new Square(file, rank), out var piece))
                    {
                        if (emptySquares != 0)
                        {
                            fileBuilder.Append(emptySquares);
                            emptySquares = 0;
                        }

                        fileBuilder.Append(piece.ToAlgebraicNotation());
                    }
                    else
                    {
                        emptySquares++;
                    }
                }

                if (emptySquares != 0)
                {
                    fileBuilder.Append(emptySquares);
                }

                boardParts.Add(fileBuilder.ToString());
            }

            parts.Add(string.Join('/', boardParts));

            parts.Add(board.ActiveColor.Is(Piece.White) ? "w" : "b");

            var castlings = new[] {
                (Castlings.WhiteKingside, Piece.White | Piece.King),
                (Castlings.WhiteQueenside, Piece.White | Piece.Queen),
                (Castlings.BlackKingside, Piece.None | Piece.King),
                (Castlings.BlackQueenside, Piece.None | Piece.Queen)
            }.Where(c => board.Counters.Castlings.HasFlag(c.Item1)).Select(c => c.Item2.ToAlgebraicNotation()).ToArray();

            parts.Add(castlings.Length == 0 ? "-" : new string(castlings));

            parts.Add(board.Counters.EnPassantTarget == 0
                ? "-"
                : board.Counters.EnPassantTarget.ToSquare().ToAlgebraicNotation());

            parts.Add(board.Counters.HalfmoveClock.ToString());
            parts.Add(board.Counters.FullmoveNumber.ToString());

            return string.Join(" ", parts.ToArray());
        }


        private static int ParseFullmoveNumber(string s)
        { 
            var number = int.Parse(s);

            if (number < 1)
            {
                number = 1;

                Log.Warning("fullmove number should be >= 1, setting to 1");
            }

            return number;
        } 

        private static int ParseHalfmoveClock(string s) => int.Parse(s);

        private static Square? ParseEnPassantTarget(string s)
        {
            if (s.IsNothing())
            {
                return null;
            }

            if (!s.IsAlgebraicNotation())
            {
                throw new FenParsingException(s, "en passant target should be in algebraic notation or '-'");
            }

            return new Square(s);
        }

        private static Piece ParseActiveColor(string s)
        {
            if (!Regex.IsMatch(s, "^[w|b]$"))
            {
                throw new FenParsingException(s, "active color should be 'w' or 'b'");
            }

            return s.Single() switch
            {
                'w' => Piece.White,
                _ => Piece.None,
            };
        }

        private static Castlings ParseCastling(string s)
        {
            if (!Regex.IsMatch(s, "^K?Q?k?q?$") && !Regex.IsMatch(s, "^-$"))
            {
                throw new FenParsingException(s, "castling should be \"KQkq\" with omitted letters when appropriate, or \"-\" if none");
            }

            Castlings castlings = 0;

            foreach (var c in s)
            {
                switch (c)
                {
                    case 'K':
                        castlings |= Castlings.WhiteKingside;
                        break;
                    case 'Q':
                        castlings |= Castlings.WhiteQueenside;
                        break;
                    case 'k':
                        castlings |= Castlings.BlackKingside;
                        break;
                    case 'q':
                        castlings |= Castlings.BlackQueenside;
                        break;
                }
            }

            return castlings;
        }
    }
}
