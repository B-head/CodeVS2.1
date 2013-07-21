using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public void MaxRect(Rect from)
        {
            MaxRect(from.Left, from.Top, from.Right, from.Bottom);
        }

        public void MaxRect(int left, int top, int right, int bottom)
        {
            Left = Math.Min(Left, left);
            Top = Math.Min(Top, top);
            Right = Math.Max(Right, right);
            Bottom = Math.Max(Bottom, bottom);
        }

        public bool IsInRect(int x, int y)
        {
            return x >= Left && x <= Right && y >= Top && y <= Bottom;
        }
    }

    public struct TurnInfo
    {
        public long AppraisalScore;
        public long RawScore;
        public long ExtraScore;
        public int Chain;
        public int TotalEraseCount;
        public int EraseBlock;
        public int EraseObstacle;
        public int EraseWidth;
        public int MaxEraseCount;
        public int MaxEraseChain;
        public int MostBottom;
        public int FieldSpace;
        public int FallingCount;
        public Rect ChangeRect;

        public override string ToString()
        {
            return string.Format("AppraisalScore {0}, RawScore {1}, ExtraScore {2}, Chain {3}, TotalEraseCount {4}, " +
                "EraseBlock {5}, EraseObstacle {6}, EraseWidth {7}, MaxEraseCount {8}, MaxEraseChain {9}, MostBottom {10}, FieldSpace {11}, FallingCount {12}",
                AppraisalScore, RawScore, ExtraScore, Chain, TotalEraseCount, EraseBlock, EraseObstacle, EraseWidth, MaxEraseCount, MaxEraseChain, MostBottom, FieldSpace, FallingCount);
        }

        public string GetStringInformation()
        {
            return string.Format("AppraisalScore {0}\nRawScore {1}\nExtraScore {2}\nChain {3}\nTotalEraseCount {4}\n" +
                "EraseBlock {5}\nEraseObstacle {6}\nEraseWidth {7}\nMaxEraseCount {8}\nMaxEraseChain {9}\nMostBottom {10}\nFieldSpace {11}\nFallingCount {12}\n",
                AppraisalScore, RawScore, ExtraScore, Chain, TotalEraseCount, EraseBlock, EraseObstacle, EraseWidth, MaxEraseCount, MaxEraseChain, MostBottom, FieldSpace, FallingCount);
        }

        public static TurnInfo MaxInfo(TurnInfo a, TurnInfo b)
        {
            a.AppraisalScore = Math.Max(a.AppraisalScore, b.AppraisalScore);
            a.RawScore = Math.Max(a.RawScore, b.RawScore);
            a.ExtraScore = Math.Max(a.ExtraScore, b.ExtraScore);
            a.Chain = Math.Max(a.Chain, b.Chain);
            a.TotalEraseCount = Math.Max(a.TotalEraseCount, b.TotalEraseCount);
            a.EraseBlock = Math.Max(a.EraseBlock, b.EraseBlock);
            a.EraseObstacle = Math.Max(a.EraseObstacle, b.EraseObstacle);
            a.EraseWidth = Math.Max(a.EraseWidth, b.EraseWidth);
            a.MaxEraseCount = Math.Max(a.MaxEraseCount, b.MaxEraseCount);
            a.MaxEraseChain = Math.Max(a.MaxEraseChain, b.MaxEraseChain);
            a.MostBottom = Math.Min(a.MostBottom, b.MostBottom);
            a.FieldSpace = Math.Min(a.FieldSpace, b.FieldSpace);
            a.FallingCount = Math.Max(a.FallingCount, b.FallingCount);
            a.ChangeRect.MaxRect(b.ChangeRect);
            return a;
        }
    }

    public interface GameAI
    {
        TurnInfo NextControl(GameModel gm, out int cx, out int cr);
    }

    public class TestAI : GameAI
    {
        readonly int X;
        readonly Random random;

        public TestAI(GameModel gm)
        {
            X = gm.Setting.W - gm.Setting.T + 1;
            random = new Random(1);
        }

        public TurnInfo NextControl(GameModel gm, out int cx, out int cr)
        {
            cx = random.Next(X);
            cr = random.Next(4);
            return new TurnInfo();
        }
    }
}
