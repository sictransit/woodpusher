using SicTransit.Woodpusher.Common.Interfaces;
using SicTransit.Woodpusher.Common.Parsing.Enum;
using SicTransit.Woodpusher.Common.Parsing.Exceptions;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text.RegularExpressions;

namespace SicTransit.Woodpusher.Common.Parsing.Moves
{
    public abstract class PgnMove
    {
        private static readonly Regex PromotionRegex = new(@"^(.+)=([QRNB])$", RegexOptions.Compiled);
        private static readonly Regex AnnotationRegex = new(@"^(.+?)([?!]*)$", RegexOptions.Compiled);

        protected string Raw { get; private set; }

        public PgnAnnotation Annotation { get; private set; }

        public static PgnMove Parse(string s)
        {
            var raw = s;

            var annotationMatch = AnnotationRegex.Match(s);

            var annotation = PgnAnnotation.None;

            if (annotationMatch.Success && !string.IsNullOrEmpty(annotationMatch.Groups[2].Value))
            {
                annotation = annotationMatch.Groups[2].Value switch
                {
                    "?" => PgnAnnotation.Mistake,
                    "?!" => PgnAnnotation.Inaccuracy,
                    "??" => PgnAnnotation.Blunder,
                    _ => throw new PgnParsingException(s, $"Unknown annotation: {annotationMatch.Groups[2].Value}"),
                };

                s = annotationMatch.Groups[1].Value;
            }

            s = s.Replace("x", string.Empty);

            var promotionMatch = PromotionRegex.Match(s);

            var promotionType = Piece.None;

            if (promotionMatch.Success)
            {
                s = promotionMatch.Groups[1].Value;
                promotionType = promotionMatch.Groups[2].Value[0].ToPieceType();
            }

            PgnMove pgnMove = null;

            if (TryParseSimplePawnMove(s, out var simplePawnMove))
            {
                pgnMove = simplePawnMove!;
            }
            else if (TryParseSimplePieceMove(s, out var simplePieceMove))
            {
                pgnMove = simplePieceMove!;
            }
            else if (TryParseCastlingMove(s, out var castlingMove))
            {
                pgnMove = castlingMove!;
            }
            else if (TryParsePieceOnFileMove(s, promotionType, out var pieceOnFileMove))
            {
                pgnMove = pieceOnFileMove!;
            }
            else if (TryParsePieceOnRankMove(s, promotionType, out var pieceOnRankMove))
            {
                pgnMove = pieceOnRankMove!;
            }
            else if (TryParseFileMove(s, promotionType, out var fileMove))
            {
                pgnMove = fileMove!;
            }

            if (pgnMove != null)
            {
                pgnMove.Annotation = annotation;
                pgnMove.Raw = raw;

                return pgnMove;
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
                move = new SimplePawnMove(new Square(match.Captures[0].Value));
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
                move = new SimplePieceMove(match.Groups[1].Value[0].ToPieceType(), new Square(match.Groups[2].Value));
            }

            return move != default;
        }

        private static bool TryParsePieceOnFileMove(string s, Piece promotionType, out PieceOnFileMove? move)
        {
            move = default;

            Regex regex = new(@"^([PRNBQK])([a-h])([a-h][1-8])$"); // Nbd7

            var match = regex.Match(s);

            if (match.Success)
            {
                move = new PieceOnFileMove(match.Groups[1].Value[0].ToPieceType(), match.Groups[2].Value[0].ToFile(), new Square(match.Groups[3].Value), promotionType);
            }

            return move != default;
        }

        private static bool TryParsePieceOnRankMove(string s, Piece promotionType, out PieceOnRankMove? move)
        {
            move = default;

            Regex regex = new(@"^([PRNBQK])([1-8])([a-h][1-8])$"); // N1f3

            var match = regex.Match(s);

            if (match.Success)
            {
                move = new PieceOnRankMove(match.Groups[1].Value[0].ToPieceType(), match.Groups[2].Value[0].ToRank(), new Square(match.Groups[3].Value), promotionType);
            }

            return move != default;
        }

        private static bool TryParseFileMove(string s, Piece promotionType, out PieceOnFileMove? move)
        {
            move = default;

            Regex regex = new(@"^([a-h])([a-h][1-8])$"); // cb5

            var match = regex.Match(s);

            if (match.Success)
            {
                move = new PieceOnFileMove(Piece.Pawn, match.Groups[1].Value[0].ToFile(), new Square(match.Groups[2].Value), promotionType);
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
                    move = new CastlingKingMove();
                }
                else
                {
                    move = new CastlingQueenMove();
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
