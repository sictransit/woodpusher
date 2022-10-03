using SicTransit.Woodpusher.Model;
using SicTransit.Woodpusher.Model.Enums;
using SicTransit.Woodpusher.Model.Extensions;
using System.Text;

namespace SicTransit.Woodpusher.Engine
{
    internal class Node
    {
        public Node(Move move, int level)
        {
            Nodes = new List<Node>();
            Move = move;
            Level = level;
            Score = int.MinValue;
        }

        private int Sign => Move.Position.Piece.Color == PieceColor.White ? 1 : -1;

        public List<Node> Nodes { get; }

        private Node? PreferredDescendant => Nodes.OrderByDescending(n => n.Score * Sign).FirstOrDefault();

        public string GetLine()
        {
            var sb = new StringBuilder();

            var descendant = this;

            while (descendant != null)
            {
                sb.Append($" {descendant.Move.ToAlgebraicMoveNotation()}");

                descendant = descendant.PreferredDescendant;
            }

            return sb.ToString().Trim();
        }

        public Move Move { get; }
        public int Level { get; }
        public int Hash { get; set; }

        public int Score { get; set; }

        public override string ToString()
        {
            return $"{Move} {Score}";
        }

    }
}
