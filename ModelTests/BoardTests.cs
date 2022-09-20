﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SicTransit.Woodpusher.Common;
using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using SicTransit.Woodpusher.Parsing;
using System.Diagnostics;

namespace SicTransit.Woodpusher.Tests
{
    [TestClass()]
    public class BoardTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Logging.EnableUnitTestLogging(Serilog.Events.LogEventLevel.Debug);
        }

        [TestMethod]
        public void BoardTest()
        {
            var board = new Board();

            var whiteKing = new Piece(PieceType.King, PieceColor.White);
            var blackKing = new Piece(PieceType.King, PieceColor.Black);

            var e1 = new Square("e1");

            board = board.AddPiece(e1, whiteKing);

            var e8 = new Square("e8");

            board = board.AddPiece(e8, blackKing);

            Assert.AreEqual(whiteKing, board.Get(e1));
            Assert.AreEqual(blackKing, board.Get(e8));

            board = board.RemovePiece(e1, whiteKing);
            board = board.RemovePiece(e8, blackKing);

            Assert.IsFalse(board.IsOccupied(e1));
            Assert.IsFalse(board.IsOccupied(e8));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void OccupiedSquareTest()
        {
            var board = new Board();

            var e1 = new Square("e1");
            var whiteKing = new Piece(PieceType.King, PieceColor.White);

            board = board.AddPiece(e1, whiteKing);
            board.AddPiece(e1, whiteKing);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EmptySquareTest()
        {
            var board = new Board();

            var e1 = new Square("e1");

            board.RemovePiece(e1, new Piece(PieceType.King, PieceColor.White));
        }

        [TestMethod]
        public void FillTest()
        {
            var board = new Board();

            var whitePawn = new Piece(PieceType.Pawn, PieceColor.White);

            for (var f = 0; f < 8; f++)
            {
                for (var r = 0; r < 8; r++)
                {
                    var square = new Square(f, r);

                    board = board.AddPiece(square, whitePawn);

                    Assert.AreEqual(whitePawn, board.Get(square));
                }
            }
        }

        [TestMethod]
        public void GetPositionsTest()
        {
            var board = new Board();

            var whiteKing = new Piece(PieceType.King, PieceColor.White);
            var e1 = new Square("e1");

            var blackKing = new Piece(PieceType.King, PieceColor.Black);
            var e8 = new Square("e8");

            board = board.AddPiece(e1, whiteKing).AddPiece(e8, blackKing);

            var whitePositions = board.GetPositions(PieceColor.White).ToArray();

            Assert.AreEqual(whiteKing, whitePositions.Single().Piece);
            Assert.AreEqual(e1, whitePositions.Single().Square);

            var blackPositions = board.GetPositions(PieceColor.Black).ToArray();

            Assert.AreEqual(blackKing, blackPositions.Single().Piece);
            Assert.AreEqual(e8, blackPositions.Single().Square);
        }

        [TestMethod]
        public void GetPositionsOnFileByTypeTest()
        {
            var board = new Board();

            var blackPawn1 = new Piece(PieceType.Pawn, PieceColor.Black);
            var e2 = new Square("e2");

            var blackPawn2 = new Piece(PieceType.Pawn, PieceColor.Black);
            var e3 = new Square("e3");

            var blackPawn3 = new Piece(PieceType.Pawn, PieceColor.Black);
            var e4 = new Square("e4");

            board = board.AddPiece(e2, blackPawn1).AddPiece(e3, blackPawn2).AddPiece(e4, blackPawn3);

            var positions = board.GetPositions(PieceColor.Black, PieceType.Pawn, 4).ToArray();

            Assert.AreEqual(3, positions.Length);

            Assert.IsTrue(positions.All(p => p.Piece.Type == PieceType.Pawn));
            Assert.IsTrue(positions.All(p => new[] { e2, e3, e4 }.Contains(p.Square)));
        }

        [TestMethod]
        public void GetPositionsOnFileTest()
        {
            Board board = new();

            var pieces = new HashSet<Piece>();

            var file = 3;
            var rank = 0;

            foreach (PieceType pieceType in Enum.GetValues(typeof(PieceType)))
            {
                if (pieceType != PieceType.None)
                {
                    var piece = new Piece(pieceType, PieceColor.Black);
                    pieces.Add(piece);
                    board = board.AddPiece(new Square(file, rank++), piece);
                }
            }

            foreach (var position in board.GetPositions(PieceColor.Black, file))
            {
                Assert.IsTrue(pieces.Contains(position.Piece));
            }

            Assert.AreEqual(0, board.GetPositions(PieceColor.White, file).Count());
        }

        [TestMethod]
        public void PlayTest()
        {
            var whiteBishop = new Piece(PieceType.Bishop, PieceColor.White);
            var blackPawn = new Piece(PieceType.Pawn, PieceColor.Black);

            var c1 = new Square("c1");
            var g7 = new Square("g7");

            var board = new Board().AddPiece(c1, whiteBishop).AddPiece(g7, blackPawn);

            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(0, board.Counters.FullmoveNumber);
            Assert.AreEqual(PieceColor.White, board.Counters.ActiveColor);
            Assert.IsNull(board.Counters.EnPassantTarget);

            Trace.WriteLine(board.PrettyPrint());

            var d2 = new Square("d2");

            board = board.Play(new(new(whiteBishop, c1), new(d2)));
            Assert.AreEqual(PieceColor.Black, board.Counters.ActiveColor);
            Assert.AreEqual(1, board.Counters.HalfmoveClock);
            Assert.AreEqual(0, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());

            var g5 = new Square("g5");

            board = board.Play(new(new(blackPawn, g7), new(g5, SpecialMove.CannotTake, new Square("g6"))));
            Assert.AreEqual(PieceColor.White, board.Counters.ActiveColor);
            Assert.AreEqual(new Square("g6"), board.Counters.EnPassantTarget);
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(1, board.Counters.FullmoveNumber);

            Trace.WriteLine(board.PrettyPrint());

            board = board.Play(new(new(whiteBishop, d2), new(g5)));
            Assert.IsNull(board.Counters.EnPassantTarget);
            Assert.AreEqual(0, board.Counters.HalfmoveClock);
            Assert.AreEqual(1, board.Counters.FullmoveNumber);

            Assert.AreEqual(0, board.GetPositions(PieceColor.Black).Count());
            Assert.AreEqual(1, board.GetPositions(PieceColor.White).Count());

            Assert.AreEqual(whiteBishop, board.Get(g5));

            Trace.WriteLine(board.PrettyPrint());
        }

        [TestMethod()]
        public void GetAttackersTest()
        {
            var board = ForsythEdwardsNotation.Parse("5r2/2Q2n2/5k2/7r/P3P1p1/1B6/5P2/6K1 b - a3 0 34");

            var attackers = board.GetAttackers(new Square("f7")).ToArray();

            Assert.AreEqual(2, attackers.Length);
            Assert.IsTrue(attackers.Any(a => a.Piece.Type == PieceType.Queen && a.Piece.Color == PieceColor.White));
            Assert.IsTrue(attackers.Any(a => a.Piece.Type == PieceType.Bishop && a.Piece.Color == PieceColor.White));
        }

        [TestMethod]
        public void MovesFromStartingPositionTest()
        {
            var board = ForsythEdwardsNotation.Parse(ForsythEdwardsNotation.StartingPosition);

            var moves = board.GetValidMoves().ToArray();

            Assert.AreEqual(20, moves.Length);
            Assert.AreEqual(16, moves.Count(p => p.Position.Piece.Type == PieceType.Pawn));
            Assert.AreEqual(4, moves.Count(p => p.Position.Piece.Type == PieceType.Knight));
        }

        [TestMethod]
        public void MovesFromSicilianOpeningTest()
        {
            var fen = @"rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1";

            var board = ForsythEdwardsNotation.Parse(fen);

            var moves = board.GetValidMoves().ToArray();

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

            var board = ForsythEdwardsNotation.Parse(fen);

            var moves = board.GetValidMoves().ToArray();

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

            var board = ForsythEdwardsNotation.Parse(fen);

            var moves = board.GetValidMoves().ToArray();

            var enPassantMove = moves.SingleOrDefault(p => p.Target.Flags.HasFlag(SpecialMove.EnPassant));

            Assert.IsNotNull(enPassantMove);

            Assert.AreEqual(new Square("f6"), enPassantMove.Target.Square);
        }

        [TestMethod]
        public void BlackCannotCastleTest()
        {
            var fen = @"r1bqk2r/pppp1Npp/2n2n2/2b4Q/2B1P3/8/PPPP1PPP/RNB2RK1 b - - 0 7";

            var board = ForsythEdwardsNotation.Parse(fen);

            var moves = board.GetValidMoves().ToArray();

            Assert.AreEqual(PieceColor.Black, board.ActiveColor);

            Assert.IsTrue(!moves.Any(p => p.Position.Piece.Type == PieceType.King && p.Target.Flags.HasFlag(SpecialMove.CastleKing)));
        }

        [TestMethod]
        public void BlackCanBlockCheckTest()
        {
            //  +---+---+---+---+---+---+---+---+
            //  | r | n | b | q | k | b | n | r | 8
            //  +---+---+---+---+---+---+---+---+
            //  | p | p | p |   | p | p | p | p | 7
            //  +---+---+---+---+---+---+---+---+
            //  |   |   |   | p |   |   |   |   | 6
            //  +---+---+---+---+---+---+---+---+
            //  |   |   |   |   |   |   |   |   | 5
            //  +---+---+---+---+---+---+---+---+
            //  | Q |   |   |   |   |   |   |   | 4
            //  +---+---+---+---+---+---+---+---+
            //  |   |   | P |   |   |   |   |   | 3
            //  +---+---+---+---+---+---+---+---+
            //  | P | P |   | P | P | P | P | P | 2
            //  +---+---+---+---+---+---+---+---+
            //  | R | N | B |   | K | B | N | R | 1
            //  +---+---+---+---+---+---+---+---+
            //    a   b   c   d   e   f   g   h
            // 
            // Fen: rnbqkbnr/ppp1pppp/3p4/8/Q7/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 2
            // Key: 5E2F301470264169
            // Checkers: a4

            var board = ForsythEdwardsNotation.Parse(@"rnbqkbnr/ppp1pppp/3p4/8/Q7/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 2");

            Log.Information(Environment.NewLine + board.PrettyPrint());

            var validMoves = board.GetValidMoves().ToArray();

            Assert.IsTrue(validMoves.Any(m => m.ToAlgebraicMoveNotation() == "b7b5"));
        }
    }
}