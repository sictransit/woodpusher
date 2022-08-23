using SicTransit.Woodpusher.Model;

namespace SicTransit.Woodpusher.Engine.Movement
{
    public class RookMovement : MovementBase
    {
        public override int Directions => 3;

        public override IEnumerable<Square> GetTargetSquares(Square square, int direction)
        {
            switch (direction)
            {
                case 0:
                    for (int r = square.Rank + 1; r <= 7; r++)
                    {
                        yield return square.NewRank(r);
                    }
                    break;
                case 1:
                    for (int f = square.File + 1; f <= 7; f++)
                    {
                        yield return square.NewFile(f);
                    }
                    break;
                case 2:
                    for (int r = square.Rank - 1; r >= 0; r--)
                    {
                        yield return square.NewRank(r);
                    }
                    break;
                case 3:
                    for (int f = square.File - 1; f >= 0; f--)
                    {
                        yield return square.NewFile(f);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
