using Microsoft.VisualStudio.TestTools.UnitTesting;
using SicTransit.Woodpusher.Engine;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Parsing;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class PatzerTests
    {
        private Patzer? patzer;

        [TestInitialize]
        public void Initialize()
        {
            patzer = new Patzer();
        }

        [TestMethod]
        public void InitializeTest()
        {
            Assert.IsNotNull(patzer!.Board);
            Assert.AreEqual(PieceColour.White, patzer.Board.ActiveColour);
            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.WhiteCastlings);
            Assert.AreEqual(Castlings.Kingside | Castlings.Queenside, patzer.Board.Counters.BlackCastlings);
        }

        [TestMethod]
        public void MovesFromStartingPositionTest()
        {
            var moves = patzer!.GetValidMoves(patzer.Board.ActiveColour).ToArray();

            Assert.AreEqual(20, moves.Length);
            Assert.AreEqual(16, moves.Count(p => p.Position.Piece.Type == PieceType.Pawn));
            Assert.AreEqual(4, moves.Count(p => p.Position.Piece.Type == PieceType.Knight));
        }

        [TestMethod]
        public void MovesFromSicilianOpeningTest()
        {
            var fen = @"rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1";

            patzer!.Initialize(fen);

            var moves = patzer.GetValidMoves(patzer.Board.ActiveColour).ToArray();

            Assert.AreEqual(15, moves.Count(p => p.Position.Piece.Type == PieceType.Pawn));
            Assert.AreEqual(1, moves.Count(p => p.Position.Piece.Type == PieceType.King));
            Assert.AreEqual(5, moves.Count(p => p.Position.Piece.Type == PieceType.Knight));
            Assert.AreEqual(5, moves.Count(p => p.Position.Piece.Type == PieceType.Bishop));
            Assert.AreEqual(4, moves.Count(p => p.Position.Piece.Type == PieceType.Queen));
        }

        [TestMethod]
        public void WhiteCastlingKingsideTest()
        {
            var fen = @"r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4";

            patzer!.Initialize(fen);

            var moves = patzer.GetValidMoves(patzer.Board.ActiveColour).ToArray();

            Assert.AreEqual(11, moves.Count(p => p.Position.Piece.Type == PieceType.Pawn));
            Assert.AreEqual(1, moves.Count(p => p.Position.Piece.Type == PieceType.Queen));
            Assert.AreEqual(7, moves.Count(p => p.Position.Piece.Type == PieceType.Knight));
            Assert.AreEqual(9, moves.Count(p => p.Position.Piece.Type == PieceType.Bishop));
            Assert.AreEqual(2, moves.Count(p => p.Position.Piece.Type == PieceType.Rook));
            Assert.AreEqual(3, moves.Count(p => p.Position.Piece.Type == PieceType.King));
            Assert.AreEqual(1, moves.Count(p => p.Position.Piece.Type == PieceType.King && p.Target.Flags.HasFlag(SpecialMove.CastleKing)));
        }

        [TestMethod]
        public void EnPassantWithoutTargetTest()
        {
            var fen = @"r1bqkb1r/ppp1p1pp/2np3n/3PPp2/8/8/PPP2PPP/RNBQKBNR w KQkq f6 0 5";

            patzer!.Initialize(fen);

            var moves = patzer.GetValidMoves(patzer.Board.ActiveColour).ToArray();

            var enPassantMove = moves.SingleOrDefault(p => p.Target.Flags.HasFlag(SpecialMove.EnPassant));

            Assert.IsNotNull(enPassantMove);

            Assert.AreEqual(new Square("f6"), enPassantMove.Target.Square);
        }

        [TestMethod]
        public void BlackCannotCastleTest()
        {
            var fen = @"r1bqk2r/pppp1Npp/2n2n2/2b4Q/2B1P3/8/PPPP1PPP/RNB2RK1 b - - 0 7";

            patzer!.Initialize(fen);

            var moves = patzer.GetValidMoves(patzer.Board.ActiveColour).ToArray();

            Assert.AreEqual(PieceColour.Black, patzer.Board.ActiveColour);

            Assert.IsTrue(!moves.Any(p => p.Position.Piece.Type == PieceType.King && p.Target.Flags.HasFlag(SpecialMove.CastleKing)));
        }

        [TestMethod]
        public void IsCheckedTest()
        {
            var fen = @"rnbqk2r/pppp2pp/4pp2/8/1b1PPP1P/2P2n2/PP4P1/RNBQKBNR w KQkq - 1 8";

            patzer!.Initialize(fen);

            Assert.IsTrue(patzer.IsChecked(patzer.Board.FindKing(PieceColour.White)));

        }

        [TestMethod]
        public void IsNotCheckedTest()
        {
            patzer!.Initialize(ForsythEdwardsNotation.StartingPosition);

            Assert.IsFalse(patzer.IsChecked(patzer.Board.FindKing(PieceColour.White)));
        }

    }
}