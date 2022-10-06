using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public abstract class PgnMove
    {
        private static readonly Regex PromotionRegex = new(@"^(.+)=([QRNB])$", RegexOptions.Compiled);

        protected string Raw { get; }

        protected PgnMove(string raw)
        {
            Raw = raw;
        }

        public static PgnMove Parse(string s)
        {
            s = s.Replace("x", string.Empty);

            var promotionMatch = PromotionRegex.Match(s);

            var promotionType = PieceType.None;

            if (promotionMatch.Success)
            {
                s = promotionMatch.Groups[1].Value;
                promotionType = promotionMatch.Groups[2].Value[0].ToPieceType();
            }

            if (TryParseSimplePawnMove(s, out var simplePawnMove))
            {
                return simplePawnMove!;
            }

            if (TryParseSimplePieceMove(s, out var simplePieceMove))
            {
                return simplePieceMove!;
            }

            if (TryParseCastlingMove(s, out var castlingMove))
            {
                return castlingMove!;
            }

            if (TryParsePieceOnFileMove(s, promotionType, out var pieceOnFileMove))
            {
                return pieceOnFileMove!;
            }

            if (TryParsePieceOnRankMove(s, promotionType, out var pieceOnRankMove))
            {
                return pieceOnRankMove!;
            }


            if (TryParseFileMove(s, promotionType, out var fileMove))
            {
                return fileMove!;
            }


            throw new ArgumentException($"unable to parse: [{s}]");
        }

        private static bool TryParseSimplePawnMove(string s, out SimplePawnMove? move)
        {
            move = default;

            Regex regex = new(@"^([a-h][1-8])$"); // e4

            var match = regex.Match(s);

            if (match.Success)
            {
                move = new SimplePawnMove(s, new Square(match.Captures[0].Value));
            }

            return move != default;
        }

        private static bool TryParseSimplePieceMove(string s, out SimplePieceMove? move)
        {
            move = default;

            Regex regex = new(@"^([PRNBQK])([a-h][1-8])$"); // Nf3

            var match = regex.Match(s);

            if (match.Success)
            {
                move = new SimplePieceMove(s, match.Groups[1].Value[0].ToPieceType(), new Square(match.Groups[2].Value));
            }

            return move != default;
        }

        private static bool TryParsePieceOnFileMove(string s, PieceType promotionType, out PieceOnFileMove? move)
        {
            move = default;

            Regex regex = new(@"^([PRNBQK])([a-h])([a-h][1-8])$"); // Nbd7

            var match = regex.Match(s);

            if (match.Success)
            {
                move = new PieceOnFileMove(s, match.Groups[1].Value[0].ToPieceType(), match.Groups[2].Value[0].ToFile(), new Square(match.Groups[3].Value), promotionType);
            }

            return move != default;
        }

        private static bool TryParsePieceOnRankMove(string s, PieceType promotionType, out PieceOnRankMove? move)
        {
            move = default;

            Regex regex = new(@"^([PRNBQK])([1-8])([a-h][1-8])$"); // N1f3

            var match = regex.Match(s);

            if (match.Success)
            {
                move = new PieceOnRankMove(s, match.Groups[1].Value[0].ToPieceType(), match.Groups[2].Value[0].ToRank(), new Square(match.Groups[3].Value), promotionType);
            }

            return move != default;
        }

        private static bool TryParseFileMove(string s, PieceType promotionType, out PieceOnFileMove? move)
        {
            move = default;

            Regex regex = new(@"^([a-h])([a-h][1-8])$"); // cb5

            var match = regex.Match(s);

            if (match.Success)
            {
                move = new PieceOnFileMove(s, PieceType.Pawn, match.Groups[1].Value[0].ToFile(), new Square(match.Groups[2].Value), promotionType);
            }

            return move != default;
        }


        private static bool TryParseCastlingMove(string s, out CastlingMove? move)
        {
            move = default;

            Regex regex = new(@"^(O\-O)(\-O)?$"); // O-O[-O]

            var match = regex.Match(s);

            if (match.Success)
            {
                if (string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    move = new CastlingKingMove(s);
                }
                else
                {
                    move = new CastlingQueenMove(s);
                }
            }

            return move != default;
        }

        public Move GetMove(IEngine engine) => CreateMove(engine);

        protected abstract Move CreateMove(IEngine engine);

        public override string ToString()
        {
            return Raw;
        }
    }
}
